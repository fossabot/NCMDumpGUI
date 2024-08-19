namespace NCMDumpGUI
{
    partial class WndMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            filepathTextBox = new TextBox();
            browseButton = new Button();
            convertButton = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            fixMetaDataCheckBox = new CheckBox();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(154, 24);
            label1.TabIndex = 0;
            label1.Text = "请输入文件路径：";
            // 
            // filepathTextBox
            // 
            filepathTextBox.Location = new Point(16, 36);
            filepathTextBox.Name = "filepathTextBox";
            filepathTextBox.Size = new Size(452, 30);
            filepathTextBox.TabIndex = 1;
            // 
            // browseButton
            // 
            browseButton.Location = new Point(474, 36);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(45, 34);
            browseButton.TabIndex = 2;
            browseButton.Text = "...";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // convertButton
            // 
            convertButton.Location = new Point(16, 72);
            convertButton.Name = "convertButton";
            convertButton.Size = new Size(112, 34);
            convertButton.TabIndex = 3;
            convertButton.Text = "转换";
            convertButton.UseVisualStyleBackColor = true;
            convertButton.Click += convertButton_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(24, 24);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2 });
            statusStrip1.Location = new Point(0, 119);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(531, 32);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(66, 25);
            toolStripStatusLabel1.Text = "状态：";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(100, 25);
            toolStripStatusLabel2.Text = "请提供文件";
            // 
            // fixMetaDataCheckBox
            // 
            fixMetaDataCheckBox.AutoSize = true;
            fixMetaDataCheckBox.Checked = true;
            fixMetaDataCheckBox.CheckState = CheckState.Checked;
            fixMetaDataCheckBox.Location = new Point(392, 78);
            fixMetaDataCheckBox.Name = "fixMetaDataCheckBox";
            fixMetaDataCheckBox.Size = new Size(126, 28);
            fixMetaDataCheckBox.TabIndex = 5;
            fixMetaDataCheckBox.Text = "修复元数据";
            fixMetaDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // WndMain
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(531, 151);
            Controls.Add(fixMetaDataCheckBox);
            Controls.Add(statusStrip1);
            Controls.Add(convertButton);
            Controls.Add(browseButton);
            Controls.Add(filepathTextBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "WndMain";
            Text = "NCMDumpGUI v1.0.0.0";
            KeyDown += WndMain_KeyDown;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox filepathTextBox;
        private Button browseButton;
        private Button convertButton;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private CheckBox fixMetaDataCheckBox;
    }
}
