require("dotenv").config(); // .env'i en üstte yükle, diğer tüm require'lardan önce

const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const path = require("path");
const axios = require("axios");
const fs = require('fs');

const app = express();
const PORT = process.env.PORT || 8080;

app.use(bodyParser.json());
app.use(cors());

// Bağlı kullanıcıları depolamak için basit bir dizi
const connectedUsers = [];

// Anahtar yakalama ve yönlendirme middleware'i
app.get("/:key", (req, res, next) => {
    const keyPattern = /^\d{8}$/; // 8 basamaklı anahtar deseni
    if (keyPattern.test(req.params.key)) {
        res.sendFile(path.join(__dirname, "index.html"));
    } else {
        next();
    }
});

// Veri dosyası yolu
const dataFilePath = path.join(__dirname, 'userLoginData.CY');

// QR kod verisini dosyaya ekleme
app.post('/kaydet', (req, res) => {
    const qrData = req.body.qrData;

    if (!qrData) {
        return res.status(400).json({ error: 'Geçersiz veri' });
    }

    // Mevcut verileri oku
    fs.readFile(dataFilePath, 'utf8', (err, data) => {
        if (err) {
            return res.status(500).json({ error: 'Veri okunamadı' });
        }

        // Veriyi kontrol et
        const existingData = data.split('\n').map(line => line.trim()).filter(Boolean);
        
        if (existingData.includes(qrData)) {
            return res.status(409).json({ error: 'Veri zaten mevcut' });
        }

        // Boş satır varsa yeni veriyi oraya ekle
        const updatedData = existingData.join('\n') + (existingData.length > 0 ? '\n' : '') + qrData;
        
        // Dosyayı güncelle
        fs.writeFile(dataFilePath, updatedData + '\n', (err) => {
            if (err) {
                return res.status(500).json({ error: 'Veri kaydedilemedi' });
            }
            res.json({ success: 'Veri kaydedildi' });
        });
    });
});

// QR kod verisini dosyadan silme (GET isteği ile)
app.get('/sil/*', (req, res) => {
    const qrDataParams = decodeURIComponent(req.params[0]); // Tüm parametreleri al
    const qrDataToDelete = qrDataParams.replace(/ /g, ''); // Gelen verideki boşlukları kaldır

    if (!qrDataToDelete) {
        return res.status(400).json({ error: 'Geçersiz veri' });
    }

    // Dosyadaki verileri oku
    fs.readFile(dataFilePath, 'utf-8', (err, data) => {
        if (err) {
            return res.status(500).json({ error: 'Veri okunamadı' });
        }

        // Dosyadan okunan verileri kontrol et
        const dataLines = data.split('\n').map(line => line.trim().replace(/ /g, '')).filter(line => line !== '');
        const filteredData = dataLines.filter(line => line !== qrDataToDelete); // Boşlukları kaldırarak karşılaştır

        if (dataLines.length === filteredData.length) {
            return res.json({ error: 'Veri bulunamadı' }); // İçerik yoksa mesaj döndür
        }

        // Filtrelenmiş veriyi dosyaya geri yaz
        fs.writeFile(dataFilePath, filteredData.join('\n'), (err) => {
            if (err) {
                return res.status(500).json({ error: 'Veri silinemedi' });
            }
            res.json({ success: 'Veri başarıyla silindi' }); // Başarı mesajı döndür
        });
    });
});
// Verilerin varlığını kontrol etme
app.get('/kontrol/*', (req, res) => {
    const qrDataParams = decodeURIComponent(req.params[0]); // Tüm parametreleri al
    const qrDataToCheck = qrDataParams.replace(/ /g, ''); // Gelen verideki boşlukları kaldır

    fs.readFile(dataFilePath, 'utf-8', (err, data) => {
        if (err) {
            return res.status(500).json({ error: 'Veri okunamadı' });
        }

        // Dosyadaki verileri oku ve boşlukları kaldır
        const dataLines = data.split('\n').map(line => line.trim().replace(/ /g, '')).filter(line => line !== '');

        // Kontrol et
        const exists = dataLines.includes(qrDataToCheck); // Boşlukları yok sayarak kontrol et

        return res.json({ qrData: qrDataToCheck, exists }); // Sonucu döndür
    });
});


// Ana sayfa endpointi
app.get("/", async (req, res) => {
    try {
        const clientIP = req.headers["x-forwarded-for"] || req.connection.remoteAddress;
        const response = await axios.get(`https://ipinfo.io/${clientIP}`);
        const { city, country } = response.data;
        console.log("Yeni bir kullanıcı bağlandı. IP Adresi: " + clientIP + "  Ülke: " + city + ", " + country);
    } catch (error) {
        console.error("Konum alınamadı:", error.message);
    }

    res.sendFile(path.join(__dirname, "index.html"));
});

// Kullanıcı bağlandığında bilgileri ekleyen endpoint
app.post("/connect", (req, res) => {
    const { username } = req.body;

    if (username) {
        connectedUsers.push(username);
        res.status(200).json({ message: "Bağlandı", connectedUsers });
    } else {
        res.status(400).json({ error: "Geçersiz istek" });
    }
});

// Bağlı olan kullanıcıları gösteren endpoint
app.get("/connected-users", (req, res) => {
    res.status(200).json({ connectedUsers });
});

// Sunucuyu başlatma
app.listen(PORT, () => {
    console.log(`Server is running at http://localhost:${PORT}`);
});
