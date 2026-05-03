namespace PCDisableCY
{
    partial class VoteSystem
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            textBoxSecenek = new TextBox();
            lblAnketKodu = new Label();
            listBoxSecenekler = new ListBox();
            button2 = new Button();
            button3 = new Button();
            resultListBox = new ListBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(153, 36);
            button1.TabIndex = 0;
            button1.Text = "Anketi oluştur";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnAnketOlustur_Click;
            // 
            // textBoxSecenek
            // 
            textBoxSecenek.Location = new Point(22, 87);
            textBoxSecenek.Name = "textBoxSecenek";
            textBoxSecenek.Size = new Size(100, 23);
            textBoxSecenek.TabIndex = 1;
            // 
            // lblAnketKodu
            // 
            lblAnketKodu.AutoSize = true;
            lblAnketKodu.Location = new Point(255, 12);
            lblAnketKodu.Name = "lblAnketKodu";
            lblAnketKodu.Size = new Size(79, 15);
            lblAnketKodu.TabIndex = 2;
            lblAnketKodu.Text = "lblAnketKodu";
            // 
            // listBoxSecenekler
            // 
            listBoxSecenekler.FormattingEnabled = true;
            listBoxSecenekler.ItemHeight = 15;
            listBoxSecenekler.Location = new Point(71, 207);
            listBoxSecenekler.Name = "listBoxSecenekler";
            listBoxSecenekler.Size = new Size(120, 94);
            listBoxSecenekler.TabIndex = 3;
            // 
            // button2
            // 
            button2.Location = new Point(128, 87);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 4;
            button2.Text = "ekle";
            button2.UseVisualStyleBackColor = true;
            button2.Click += btnEkle_Click;
            // 
            // button3
            // 
            button3.Location = new Point(508, 148);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 5;
            button3.Text = "Bitir";
            button3.UseVisualStyleBackColor = true;
            button3.Click += btnAnketBitir_Click;
            // 
            // resultListBox
            // 
            resultListBox.FormattingEnabled = true;
            resultListBox.ItemHeight = 15;
            resultListBox.Location = new Point(430, 286);
            resultListBox.Name = "resultListBox";
            resultListBox.Size = new Size(120, 94);
            resultListBox.TabIndex = 6;
            // 
            // VoteSystem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(resultListBox);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(listBoxSecenekler);
            Controls.Add(lblAnketKodu);
            Controls.Add(textBoxSecenek);
            Controls.Add(button1);
            Name = "VoteSystem";
            Text = "VoteSystem";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBoxSecenek;
        private Label lblAnketKodu;
        private ListBox listBoxSecenekler;
        private Button button2;
        private Button button3;
        private ListBox resultListBox;
    }
}