﻿namespace InsightLogParser.UI {
    partial class Main {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            pnlMain = new TableLayoutPanel();
            pnlTarget = new TableLayoutPanel();
            lbl2DDistance = new Label();
            lblVerticalDistance = new Label();
            picPuzzleType = new PictureBox();
            picCompass = new PictureBox();
            picArrow = new PictureBox();
            lblPuzzleType = new Label();
            lblID = new Label();
            pnlRoute = new TableLayoutPanel();
            picScreenshot = new PictureBox();
            picMap = new PictureBox();
            strStatus = new StatusStrip();
            lblIP = new ToolStripStatusLabel();
            pnlMain.SuspendLayout();
            pnlTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPuzzleType).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCompass).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picArrow).BeginInit();
            pnlRoute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picScreenshot).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMap).BeginInit();
            strStatus.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.ColumnCount = 1;
            pnlMain.ColumnStyles.Add(new ColumnStyle());
            pnlMain.Controls.Add(pnlTarget, 0, 0);
            pnlMain.Controls.Add(pnlRoute, 0, 1);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(0);
            pnlMain.Name = "pnlMain";
            pnlMain.RowCount = 2;
            pnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            pnlMain.RowStyles.Add(new RowStyle());
            pnlMain.Size = new Size(814, 447);
            pnlMain.TabIndex = 0;
            // 
            // pnlTarget
            // 
            pnlTarget.ColumnCount = 7;
            pnlTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            pnlTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            pnlTarget.ColumnStyles.Add(new ColumnStyle());
            pnlTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            pnlTarget.ColumnStyles.Add(new ColumnStyle());
            pnlTarget.ColumnStyles.Add(new ColumnStyle());
            pnlTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            pnlTarget.Controls.Add(lbl2DDistance, 2, 0);
            pnlTarget.Controls.Add(lblVerticalDistance, 4, 0);
            pnlTarget.Controls.Add(picPuzzleType, 0, 0);
            pnlTarget.Controls.Add(picCompass, 1, 0);
            pnlTarget.Controls.Add(picArrow, 3, 0);
            pnlTarget.Controls.Add(lblPuzzleType, 5, 0);
            pnlTarget.Controls.Add(lblID, 6, 0);
            pnlTarget.Dock = DockStyle.Fill;
            pnlTarget.Location = new Point(0, 0);
            pnlTarget.Margin = new Padding(0);
            pnlTarget.Name = "pnlTarget";
            pnlTarget.RowCount = 1;
            pnlTarget.RowStyles.Add(new RowStyle());
            pnlTarget.Size = new Size(814, 64);
            pnlTarget.TabIndex = 0;
            // 
            // lbl2DDistance
            // 
            lbl2DDistance.AutoSize = true;
            lbl2DDistance.Dock = DockStyle.Fill;
            lbl2DDistance.Font = new Font("Segoe UI", 36F);
            lbl2DDistance.ForeColor = Color.White;
            lbl2DDistance.Location = new Point(128, 0);
            lbl2DDistance.Margin = new Padding(0);
            lbl2DDistance.Name = "lbl2DDistance";
            lbl2DDistance.Size = new Size(1, 65);
            lbl2DDistance.TabIndex = 0;
            // 
            // lblVerticalDistance
            // 
            lblVerticalDistance.AutoSize = true;
            lblVerticalDistance.BackColor = Color.Black;
            lblVerticalDistance.Dock = DockStyle.Fill;
            lblVerticalDistance.Font = new Font("Segoe UI", 36F);
            lblVerticalDistance.ForeColor = Color.White;
            lblVerticalDistance.Location = new Point(192, 0);
            lblVerticalDistance.Margin = new Padding(0);
            lblVerticalDistance.Name = "lblVerticalDistance";
            lblVerticalDistance.Size = new Size(1, 65);
            lblVerticalDistance.TabIndex = 1;
            // 
            // picPuzzleType
            // 
            picPuzzleType.Dock = DockStyle.Fill;
            picPuzzleType.Location = new Point(0, 0);
            picPuzzleType.Margin = new Padding(0);
            picPuzzleType.Name = "picPuzzleType";
            picPuzzleType.Size = new Size(64, 65);
            picPuzzleType.SizeMode = PictureBoxSizeMode.Zoom;
            picPuzzleType.TabIndex = 2;
            picPuzzleType.TabStop = false;
            // 
            // picCompass
            // 
            picCompass.Dock = DockStyle.Fill;
            picCompass.Location = new Point(64, 0);
            picCompass.Margin = new Padding(0);
            picCompass.Name = "picCompass";
            picCompass.Size = new Size(64, 65);
            picCompass.SizeMode = PictureBoxSizeMode.Zoom;
            picCompass.TabIndex = 3;
            picCompass.TabStop = false;
            // 
            // picArrow
            // 
            picArrow.Dock = DockStyle.Fill;
            picArrow.Location = new Point(128, 0);
            picArrow.Margin = new Padding(0);
            picArrow.Name = "picArrow";
            picArrow.Size = new Size(64, 65);
            picArrow.SizeMode = PictureBoxSizeMode.Zoom;
            picArrow.TabIndex = 4;
            picArrow.TabStop = false;
            // 
            // lblPuzzleType
            // 
            lblPuzzleType.AutoSize = true;
            lblPuzzleType.Dock = DockStyle.Fill;
            lblPuzzleType.Font = new Font("Segoe UI", 36F);
            lblPuzzleType.ForeColor = Color.White;
            lblPuzzleType.Location = new Point(192, 0);
            lblPuzzleType.Margin = new Padding(0);
            lblPuzzleType.Name = "lblPuzzleType";
            lblPuzzleType.Size = new Size(1, 65);
            lblPuzzleType.TabIndex = 5;
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Dock = DockStyle.Fill;
            lblID.Font = new Font("Segoe UI", 36F);
            lblID.ForeColor = Color.White;
            lblID.Location = new Point(192, 0);
            lblID.Margin = new Padding(0);
            lblID.Name = "lblID";
            lblID.Size = new Size(622, 65);
            lblID.TabIndex = 6;
            lblID.TextAlign = ContentAlignment.TopRight;
            // 
            // pnlRoute
            // 
            pnlRoute.ColumnCount = 2;
            pnlRoute.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pnlRoute.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67F));
            pnlRoute.Controls.Add(picScreenshot, 0, 0);
            pnlRoute.Controls.Add(picMap, 0, 0);
            pnlRoute.Dock = DockStyle.Fill;
            pnlRoute.Location = new Point(0, 64);
            pnlRoute.Margin = new Padding(0);
            pnlRoute.Name = "pnlRoute";
            pnlRoute.RowCount = 2;
            pnlRoute.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlRoute.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            pnlRoute.Size = new Size(814, 386);
            pnlRoute.TabIndex = 1;
            // 
            // picScreenshot
            // 
            picScreenshot.Dock = DockStyle.Fill;
            picScreenshot.Location = new Point(268, 0);
            picScreenshot.Margin = new Padding(0);
            picScreenshot.Name = "picScreenshot";
            picScreenshot.Size = new Size(546, 366);
            picScreenshot.SizeMode = PictureBoxSizeMode.Zoom;
            picScreenshot.TabIndex = 1;
            picScreenshot.TabStop = false;
            // 
            // picMap
            // 
            picMap.Dock = DockStyle.Fill;
            picMap.Image = Properties.Resources.Map;
            picMap.Location = new Point(0, 0);
            picMap.Margin = new Padding(0);
            picMap.Name = "picMap";
            pnlRoute.SetRowSpan(picMap, 2);
            picMap.Size = new Size(268, 386);
            picMap.SizeMode = PictureBoxSizeMode.Zoom;
            picMap.TabIndex = 0;
            picMap.TabStop = false;
            // 
            // strStatus
            // 
            strStatus.BackColor = Color.Black;
            strStatus.Items.AddRange(new ToolStripItem[] { lblIP });
            strStatus.LayoutStyle = ToolStripLayoutStyle.Flow;
            strStatus.Location = new Point(0, 447);
            strStatus.Name = "strStatus";
            strStatus.Size = new Size(814, 20);
            strStatus.SizingGrip = false;
            strStatus.TabIndex = 1;
            // 
            // lblIP
            // 
            lblIP.BackColor = Color.Black;
            lblIP.ForeColor = Color.White;
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(79, 15);
            lblIP.Text = "Disconnected";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(814, 467);
            Controls.Add(pnlMain);
            Controls.Add(strStatus);
            Name = "Main";
            Text = "InsightLogParser.Client";
            pnlMain.ResumeLayout(false);
            pnlTarget.ResumeLayout(false);
            pnlTarget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPuzzleType).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCompass).EndInit();
            ((System.ComponentModel.ISupportInitialize)picArrow).EndInit();
            pnlRoute.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picScreenshot).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMap).EndInit();
            strStatus.ResumeLayout(false);
            strStatus.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel pnlMain;
        private TableLayoutPanel pnlTarget;
        private Label lbl2DDistance;
        private Label lblVerticalDistance;
        private PictureBox picPuzzleType;
        private PictureBox picCompass;
        private PictureBox picArrow;
        private Label lblPuzzleType;
        private Label lblID;
        private TableLayoutPanel pnlRoute;
        private PictureBox picMap;
        private PictureBox picScreenshot;
        private StatusStrip strStatus;
        private ToolStripStatusLabel lblIP;
    }
}