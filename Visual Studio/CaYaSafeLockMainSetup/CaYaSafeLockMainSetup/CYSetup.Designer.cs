namespace CaYaSafeLockMainSetup
{
    partial class CYSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CYSetup));
            label1 = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
            label3 = new Label();
            textBox2 = new TextBox();
            label4 = new Label();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(31, 9);
            label1.Name = "label1";
            label1.Size = new Size(322, 21);
            label1.TabIndex = 0;
            label1.Text = "Kilit ekranında yazmasını istediğiniz yazı";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 33);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(360, 23);
            textBox1.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 59);
            label2.Name = "label2";
            label2.Size = new Size(216, 30);
            label2.TabIndex = 2;
            label2.Text = "Bu yazı kilit ekranının sol üsttünde yazar\r\nistediğiniz gibi kişileştirebilirsiniz!";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(58, 169);
            label3.Name = "label3";
            label3.Size = new Size(154, 21);
            label3.TabIndex = 0;
            label3.Text = "Anahtar kodu girin";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 193);
            textBox2.MaxLength = 8;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(246, 23);
            textBox2.TabIndex = 1;
            textBox2.KeyPress += textBox2_KeyPress;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 219);
            label4.Name = "label4";
            label4.Size = new Size(277, 60);
            label4.TabIndex = 2;
            label4.Text = "En fazla 8 basamaklı olan bu anahtar kodu \r\nhedef cihazlarını açmak için gerekir.\r\nBu kodu kaybederseniz erişiminizi kaybedebilirsiniz.\r\nBu kodu bir yere not edin.";
            // 
            // button1
            // 
            button1.Location = new Point(264, 193);
            button1.Name = "button1";
            button1.Size = new Size(108, 23);
            button1.TabIndex = 3;
            button1.Text = "Rasgele Oluştur";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(199, 390);
            button2.Name = "button2";
            button2.Size = new Size(173, 39);
            button2.TabIndex = 4;
            button2.Text = "Kurucu programını hazırla!";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // CYSetup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 441);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(label3);
            Controls.Add(textBox1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "CYSetup";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dosya Hazırlama Sistemi CaYaSafeLock";
            FormClosing += CYSetup_FormClosing;
            Load += CYSetup_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private Label label3;
        private TextBox textBox2;
        private Label label4;
        private Button button1;
        private Button button2;
    }
}