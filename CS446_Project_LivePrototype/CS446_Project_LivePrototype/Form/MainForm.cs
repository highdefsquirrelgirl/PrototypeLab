﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS446_Project_LivePrototype
{
    public partial class MainForm : Form
    {
        private MapControl mapControl;
        private GameState gameState;
        private int savedSplitterPos;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gameState = new GameState();

            mapControl = new MapControl(gameState);
            this.mainSplitContainer.Panel1.Controls.Add(mapControl);
            mapControl.Dock = DockStyle.Fill;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            // Open map tab by default
            rightPanelTabControl.SelectedIndex = 2;

            gridThicknessSlider.Minimum = 1;
            gridThicknessSlider.Maximum = MapControl.MAX_GRID_THICKNESS;

            gridScaleSlider.Minimum = MapControl.MIN_GRID_SCALE_FACTOR;
            gridScaleSlider.Maximum = MapControl.MAX_GRID_SCALE_FACTOR;
            gridScaleSlider.Value = gridScaleSlider.Minimum;
            mapControl.GridScale = gridScaleSlider.Value;

            savedSplitterPos = mainSplitContainer.SplitterDistance;
            snapTokensToGridCheckbox.CheckState = mapControl.TokenSnapToGrid ? CheckState.Checked : CheckState.Unchecked;

            gameState.TokenLibrary.TokenAddedRemovedEvent += TokenLibrary_TokenAddedRemovedEvent;
            gameState.ActiveTokens.MapTokenAddedRemovedEvent += ActiveTokens_MapTokenAddedRemovedEvent;

            loadFonts();
        }

        private void loadFonts()
        {
            try
            {
                gameState.FontCollection = new PrivateFontCollection();
                gameState.FontCollection.AddFontFile("zrnic_rg.ttf");
                gameState.MapFontFamily = new FontFamily("Zrnic Rg", gameState.FontCollection);
                gameState.MapLabelsFont = new Font(gameState.MapFontFamily, 12, FontStyle.Regular);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading fonts: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                gameState.FontCollection = null;
                gameState.MapFontFamily = null;
                gameState.MapLabelsFont = null;
            }
        }

        // Collapses right pannel
        private void mainSplitContainer_DoubleClick(object sender, EventArgs e)
        {
            int closedPos = mainSplitContainer.Width - mainSplitContainer.SplitterWidth;

            if (mainSplitContainer.SplitterDistance >= closedPos)
            {
                mainSplitContainer.SplitterDistance = mainSplitContainer.Width - savedSplitterPos;
            }
            else
            {
                savedSplitterPos = mainSplitContainer.Width - mainSplitContainer.SplitterDistance;
                mainSplitContainer.SplitterDistance = mainSplitContainer.Width - mainSplitContainer.SplitterWidth;
            }
        }

        private void loadMapBackgroundBtn_Click(object sender, EventArgs e)
        {
            if (openMapImageDialog.ShowDialog() == DialogResult.OK)
            {
                gameState.MapImageFile = openMapImageDialog.FileName;
                mapControl.UpdateBackground();
            }
        }

        private void zoomInBtn_Click(object sender, EventArgs e)
        {
            mapControl.ZoomIn();
        }

        private void zoomOutBtn_Click(object sender, EventArgs e)
        {
            mapControl.ZoomOut();
        }

        private void mainSplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void gridScaleSlider_Scroll(object sender, EventArgs e)
        {
            mapControl.GridScale = gridScaleSlider.Value;
        }

        private void gridAlphaSlider_Scroll(object sender, EventArgs e)
        {
            mapControl.GridTransparency = 255 - gridAlphaSlider.Value;
        }

        private void gridThicknessSlider_Scroll(object sender, EventArgs e)
        {
            mapControl.GridThickness = gridThicknessSlider.Value;
        }

        private void gridHorzOffset_Scroll(object sender, EventArgs e)
        {
            // mapControl.GridHorizontalOffset = (float)gridHorzOffset.Value / 10.0f;
        }

        private void gridVertOffsetSlider_Scroll(object sender, EventArgs e)
        {
            // mapControl.GridVerticalOffset = (float)gridVertOffsetSlider.Value / 10.0f;
        }

        private void rollDiceBtn_Click(object sender, EventArgs e)
        {
            int numDice = (int)NumberOfDiceUpDown.Value;
            int numSides = (int)NumberOfSidesUpDown.Value;
            String output = "";

            Random rnd = new Random();

			output += rnd.Next(1, numSides + 1);

            for (int i = numDice - 1; i > 0; i--)
                output += ",  " + rnd.Next(1, numSides + 1);

            output.Trim();

            Console.WriteLine(output);
            diceRollOutputLabel.Text = output;
        }

        private void centerViewBtn_Click(object sender, EventArgs e)
        {
            mapControl.CenterViewOnMap();
        }

        private void resetViewBtn_Click(object sender, EventArgs e)
        {
            mapControl.ZoomFactor = 1.0f;
            mapControl.CenterViewOnMap();
        }

        private void shiftGridLeftBtn_Click(object sender, EventArgs e)
        {
            mapControl.GridHorizontalOffset -= MapControl.GRID_SHIFT_STEP;
        }

        private void shiftGridUpBtn_Click(object sender, EventArgs e)
        {
            mapControl.GridVerticalOffset -= MapControl.GRID_SHIFT_STEP;
        }

        private void shiftGridRightBtn_Click(object sender, EventArgs e)
        {
            mapControl.GridHorizontalOffset += MapControl.GRID_SHIFT_STEP;
        }

        private void shiftGridDownBtn_Click(object sender, EventArgs e)
        {
            mapControl.GridVerticalOffset += MapControl.GRID_SHIFT_STEP;
        }

        private void resetGridPosBtn_Click(object sender, EventArgs e)
        {
            mapControl.GridHorizontalOffset = 0.0f;
            mapControl.GridVerticalOffset = 0.0f;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void snapTokensToGridCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            mapControl.TokenSnapToGrid = (snapTokensToGridCheckbox.CheckState == CheckState.Checked);
        }

        private void newTokenBtn_Click(object sender, EventArgs e)
        {
            EditTokenForm charForm = new EditTokenForm(gameState);
            DialogResult result = charForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                TokenData newData = charForm.GetTokenData();
                newData.CurrentHP = newData.MaxHP;
                gameState.TokenLibrary.Add(ref newData);
            }
        }

        private void editLibTokenBtn_Click(object sender, EventArgs e)
        {
            int curSelectedIndex = tokenLibList.SelectedIndex;

            if (curSelectedIndex < 0)
            {
                MessageBox.Show("No token selected.", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TokenData tokenData = gameState.TokenLibrary[(string)tokenLibList.SelectedItem];

            EditTokenForm charForm = new EditTokenForm(gameState);
            charForm.SetTokenData(ref tokenData);
            DialogResult result = charForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                tokenData = charForm.GetTokenData();
                tokenData.CurrentHP = tokenData.MaxHP;
                gameState.TokenLibrary[tokenData.Name] = tokenData;
            }
        }

        private void placeTokenOnMapBtn_Click(object sender, EventArgs e)
        {
            int curSelectedIndex = tokenLibList.SelectedIndex;

            if (curSelectedIndex < 0)
            {
                MessageBox.Show("No token selected.", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TokenData selectedToken = gameState.TokenLibrary[(string)tokenLibList.SelectedItem];

            MapToken newToken = new MapToken(mapControl, ref selectedToken, mapControl.ViewPosition);

            // Make sure token is within the map bounds
            newToken.Position = newToken.Position;

            gameState.ActiveTokens.Add(newToken);     
        }

        // This event is automatically called whenever a token is added or removed from the token library
        private void TokenLibrary_TokenAddedRemovedEvent(object sender, TokenAddedRemovedEventArgs a)
        {
            refreshTokenLibList();
        }

        private void refreshTokenLibList()
        {
            string selectedToken = null;

            if (tokenLibList.SelectedIndex >= 0)
            {
                selectedToken = (string)tokenLibList.SelectedItem;
            }

            tokenLibList.Items.Clear();

            var enumerator = gameState.TokenLibrary.GetEnumerator();

            List<string> filteredList = new List<string>();

            while (enumerator.MoveNext())
            {
                TokenData token = enumerator.Current;

                if (token.TokenType == TokenType.Player && libPlayerFilterCheck.Checked)
                {
                    filteredList.Add(token.Name);
                }
                else if (token.TokenType == TokenType.Enemy && libEnemyFilterCheck.Checked)
                {
                    filteredList.Add(token.Name);
                }
                else if (token.TokenType == TokenType.NPC && libNonplayerFilterCheck.Checked)
                {
                    filteredList.Add(token.Name);
                }
            }

            // Sort list of names
            filteredList.Sort();

            // Add each name to lib list
            foreach (string item in filteredList)
            {
                tokenLibList.Items.Add(item);
            }

            tokenLibList.SelectedItem = selectedToken;
        }

        private void libPlayerFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            refreshTokenLibList();
        }

        private void libEnemyFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            refreshTokenLibList();
        }

        private void libNonplayerFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            refreshTokenLibList();
        }

        private void tokenLibList_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    e.IsInputKey = true;
                    break;
            }
        }

        private void tokenLibList_KeyDown(object sender, KeyEventArgs e)
        {
            int curSelectedIndex = tokenLibList.SelectedIndex;

            // No token selected
            if (curSelectedIndex < 0)
            {
                return;
            }

            TokenData selectedToken = gameState.TokenLibrary[(string)tokenLibList.SelectedItem];

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    DialogResult ans = MessageBox.Show("Are you sure you want to delete the token \"" + selectedToken.Name + "\"?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (ans == DialogResult.Yes)
                    {
                        gameState.TokenLibrary.Remove(ref selectedToken);
                        refreshTokenLibList();
                    }
                    break;
            }
        }

        private void ActiveTokens_MapTokenAddedRemovedEvent(object sender, MapTokenAddedRemovedEventArgs a)
        {
            refreshActiveTokenList();
        }

        private void refreshActiveTokenList()
        {
            MapToken selectedToken = null;

            if (tokenLibList.SelectedIndex >= 0)
            {
                selectedToken = (MapToken)activeTokensList.SelectedItem;
            }

            activeTokensList.Items.Clear();

            var enumerator = gameState.ActiveTokens.GetEnumerator();

            while (enumerator.MoveNext())
            {
                MapToken token = enumerator.Current;

                if (token.TokenType == TokenType.Player && activeTokPlayerFilterCheck.Checked)
                {
                    activeTokensList.Items.Add(token);
                }
                else if (token.TokenType == TokenType.Enemy && activeTokEnemyFilterCheck.Checked)
                {
                    activeTokensList.Items.Add(token);
                }
                else if (token.TokenType == TokenType.NPC && activeTokNonPlayerFilterCheck.Checked)
                {
                    activeTokensList.Items.Add(token);
                }
            }

            activeTokensList.SelectedItem = selectedToken;
        }

        private void activeTokPlayerFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            refreshActiveTokenList();
        }

        private void activeTokEnemyFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            refreshActiveTokenList();
        }

        private void activeTokNonPlayerFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            refreshActiveTokenList();
        }

        private void actTokLocateBtn_Click(object sender, EventArgs e)
        {
            int curSelectedIndex = activeTokensList.SelectedIndex;

            // No token selected
            if (curSelectedIndex < 0)
            {
                MessageBox.Show("No token selected.", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MapToken selectedToken = (MapToken)activeTokensList.Items[curSelectedIndex];
            mapControl.ViewPosition = selectedToken.Position;
        }

        private void actTokEditBtn_Click(object sender, EventArgs e)
        {
            int curSelectedIndex = activeTokensList.SelectedIndex;

            // No token selected
            if (curSelectedIndex < 0)
            {
                MessageBox.Show("No token selected.", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MapToken selectedToken = (MapToken)activeTokensList.Items[curSelectedIndex];
            TokenData tokenData = selectedToken.GetTokenData();

            EditTokenForm charForm = new EditTokenForm(gameState);
            charForm.SetTokenData(ref tokenData);
            DialogResult result = charForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                tokenData = charForm.GetTokenData();
                selectedToken.SetTokenData(ref tokenData);
            }
        }

        private void actTokRemoveBtn_Click(object sender, EventArgs e)
        {
            int curSelectedIndex = activeTokensList.SelectedIndex;

            // No token selected
            if (curSelectedIndex < 0)
            {
                MessageBox.Show("No token selected.", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MapToken selectedToken = (MapToken)activeTokensList.Items[curSelectedIndex];
            gameState.ActiveTokens.Remove(selectedToken);
        }

		private void label4_Click(object sender, EventArgs e)
		{

		}

		private void rightPanelSplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
		{

		}
	}
}
