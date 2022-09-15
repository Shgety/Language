namespace Language
{
    partial class MainWindows
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.code_TextBox = new System.Windows.Forms.RichTextBox();
            this.runCode_BTN = new System.Windows.Forms.Button();
            this.output_TextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // code_TextBox
            // 
            this.code_TextBox.Location = new System.Drawing.Point(13, 14);
            this.code_TextBox.Name = "code_TextBox";
            this.code_TextBox.Size = new System.Drawing.Size(775, 317);
            this.code_TextBox.TabIndex = 1;
            this.code_TextBox.Text = "";
            // 
            // runCode_BTN
            // 
            this.runCode_BTN.Location = new System.Drawing.Point(707, 337);
            this.runCode_BTN.Name = "runCode_BTN";
            this.runCode_BTN.Size = new System.Drawing.Size(81, 95);
            this.runCode_BTN.TabIndex = 3;
            this.runCode_BTN.Text = "Start";
            this.runCode_BTN.UseVisualStyleBackColor = true;
            this.runCode_BTN.Click += new System.EventHandler(this.runCode_BTN_Click);
            // 
            // output_TextBox
            // 
            this.output_TextBox.Location = new System.Drawing.Point(13, 336);
            this.output_TextBox.Name = "output_TextBox";
            this.output_TextBox.ReadOnly = true;
            this.output_TextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.output_TextBox.Size = new System.Drawing.Size(688, 96);
            this.output_TextBox.TabIndex = 4;
            this.output_TextBox.Text = "";
            // 
            // MainWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.output_TextBox);
            this.Controls.Add(this.runCode_BTN);
            this.Controls.Add(this.code_TextBox);
            this.Name = "MainWindows";
            this.Text = "Form";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox code_TextBox;
        private System.Windows.Forms.Button runCode_BTN;
        private System.Windows.Forms.RichTextBox output_TextBox;
    }
}

