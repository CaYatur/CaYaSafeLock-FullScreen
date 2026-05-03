const express = require('express');
const { v4: uuidv4 } = require('uuid');
const app = express();
const http = require('http').createServer(app);
const io = require('socket.io')(http);

let anketler = {}; // Anket verilerini saklayacağız
let kullanicilar = {}; // Her kullanıcının oylama durumu

app.use(express.json());
app.use(express.static('public')); // Statik dosyaları 'public' klasöründen sun

app.post('/anket-olustur', (req, res) => {
    const anketVerisi = req.body;
    const kod = uuidv4().slice(0, 8); // 8 haneli benzersiz kod oluştur
    anketler[kod] = { ...anketVerisi, oylar: {}, durum: 'açık' }; // Durum ekleniyor

    res.json({ kod });
});

app.post('/anket-bitir/:kod', (req, res) => {
    const kod = req.params.kod;

    if (anketler[kod]) {
        anketler[kod].durum = 'kapalı'; // Durumu kapalı yap
        res.json({ message: 'Anket başarıyla kapatıldı.' });

        // Anketi 60 saniye sonra sil
        setTimeout(() => {
            delete anketler[kod];
            console.log(`Anket ${kod} silindi.`);
        }, 60000); // 60 saniye
    } else {
        res.status(404).json({ message: 'Anket bulunamadı.' });
    }
});
// Yeni bağlantı için socket oluştur
io.on('connection', (socket) => {
    // Oylama seçeneklerini ve oy verme işlevini ayarlayın
    socket.on('oy-ver', ({ kod, secim }) => {
        if (anketler[kod] && anketler[kod].durum === 'açık') {
            const kullaniciID = socket.id; // Her kullanıcı için benzersiz bir ID
            if (!kullanicilar[kullaniciID]) { // Kullanıcı daha önce oy vermediyse
                if (!anketler[kod].oylar[secim]) {
                    anketler[kod].oylar[secim] = 0;
                }
                anketler[kod].oylar[secim]++;
                kullanicilar[kullaniciID] = true; // Kullanıcıyı işaretle

                // Gerçek zamanlı oylama sonucu güncellemesi
                io.emit('oy-guncelle', { kod, oylar: anketler[kod].oylar, secenekler: anketler[kod].secenekler });
            } else {
                // Kullanıcı zaten oy verdiğinde
                socket.emit('oy-verildi', { message: 'Bu cihazda zaten oy kullanıldı.' });
            }
        }
    });

    // Anket verilerini istemciye gönder
    socket.on('anket-al', ({ kod }) => {
        const anket = anketler[kod];
        if (anket) {
            socket.emit('anket-alindi', anket); // Anket verilerini istemciye gönder
        } else {
            socket.emit('anket-bulunamadi', { message: 'Anket bulunamadı' });
        }
    });
});


app.get('/anket-sonuc/:kod', (req, res) => {
    const kod = req.params.kod;
    const anket = anketler[kod];

    if (anket) {
        if (anket.durum === 'kapalı') {
            return res.status(400).json({ message: 'Anket kapatıldı' });
        }

        // Sonuçları JSON formatında döndür
        return res.json({
            kod: kod,
            secenekler: anket.secenekler,
            oylar: anket.oylar
        });
    } else {
        res.status(404).json({ message: 'Anket bulunamadı' });
    }
});

// Anket sayfası yönlendirme
app.get('/:kod', (req, res) => {
    const kod = req.params.kod;

    // Eğer anket mevcutsa anket sayfasına yönlendir
    if (anketler[kod]) {
        res.sendFile(__dirname + '/public/anket.html'); // Anket sayfasını döndür
    } else {
        res.status(404).send('Anket bulunamadı');
    }
});

// Sonuç sayfası
app.get('/sonuc/:kod', (req, res) => {
    const kod = req.params.kod;
    const anket = anketler[kod];

    if (anket) {
        res.redirect('/sonuc/index.html'); // Sonuç sayfasına yönlendirme
    } else {
        res.status(404).send('Anket bulunamadı');
    }
});

http.listen(3000, () => {
    console.log('Server is running on port 3000');
});
