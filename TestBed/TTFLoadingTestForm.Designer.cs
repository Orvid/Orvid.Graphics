namespace TestBed
{
	partial class TTFLoadingTestForm
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
			this.OGraphicsBox = new System.Windows.Forms.PictureBox();
			this.ReferenceBox = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.TestButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.OGraphicsBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReferenceBox)).BeginInit();
			this.SuspendLayout();
			// 
			// OGraphicsBox
			// 
			this.OGraphicsBox.Location = new System.Drawing.Point(12, 33);
			this.OGraphicsBox.Name = "OGraphicsBox";
			this.OGraphicsBox.Size = new System.Drawing.Size(326, 240);
			this.OGraphicsBox.TabIndex = 0;
			this.OGraphicsBox.TabStop = false;
			// 
			// ReferenceBox
			// 
			this.ReferenceBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReferenceBox.Location = new System.Drawing.Point(344, 33);
			this.ReferenceBox.Name = "ReferenceBox";
			this.ReferenceBox.Size = new System.Drawing.Size(130, 160);
			this.ReferenceBox.TabIndex = 1;
			this.ReferenceBox.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(132, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Orvid.Graphics Rendering:";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(353, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Reference Rendering:";
			// 
			// TestButton
			// 
			this.TestButton.Location = new System.Drawing.Point(375, 198);
			this.TestButton.Name = "TestButton";
			this.TestButton.Size = new System.Drawing.Size(75, 23);
			this.TestButton.TabIndex = 4;
			this.TestButton.Text = "Test";
			this.TestButton.UseVisualStyleBackColor = true;
			this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
			// 
			// TTFLoadingTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(486, 284);
			this.Controls.Add(this.TestButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ReferenceBox);
			this.Controls.Add(this.OGraphicsBox);
			this.Name = "TTFLoadingTestForm";
			this.Text = "TTFLoadingTestForm";
			this.Load += new System.EventHandler(this.TTFLoadingTestForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.OGraphicsBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReferenceBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button TestButton;
	}
}