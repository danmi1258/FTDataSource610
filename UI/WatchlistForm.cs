using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace AmiBroker.DataSources.IB
{
    public partial class WatchlistForm : Form
    {
        /// <summary>
        /// Output value
        /// </summary>
        public int[] SelectedWatchlistIndices;

        public WatchlistForm(string databasePath)
        {
            InitializeComponent();

            labelBoxWatchlists.Text = "Database: " + databasePath;
            StringCollection watchlistNames = WatchlistNames(databasePath);

            // only the first 64 names... 
            if (watchlistNames != null && watchlistNames.Count > 0)
                for (int i = 0; i <= 63 && i < watchlistNames.Count; i++)
                    listBoxWatchlists.Items.Add(watchlistNames[i]);
            else
                MessageBox.Show("Watchlist file is missing or database is not yet saved.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            // if nothing is selected
            if (listBoxWatchlists.SelectedIndices.Count <= 0)
            {
                SelectedWatchlistIndices = null;
                return;
            }

            SelectedWatchlistIndices = new int[listBoxWatchlists.CheckedIndices.Count];
            listBoxWatchlists.CheckedIndices.CopyTo(SelectedWatchlistIndices, 0);
        }

        /// <summary>
        /// Collection watchlist names
        /// </summary>
        /// <param name="databasePath"></param>
        /// <returns></returns>
        /// <remarks>
        /// Watchlist names are not easily accessable from OLE so we read database file
        /// If AB db is modified but NOT saved yet, we read outdated data !!!
        /// </remarks>
        private StringCollection WatchlistNames(string databasePath)
        {
            const string indexRelPath = @"Watchlists\index.txt";

            if (string.IsNullOrEmpty(databasePath))
                throw new ArgumentNullException();

            StringCollection result = null;

            try
            {
                string indexFilePath = Path.Combine(databasePath, indexRelPath);
                if (File.Exists(indexFilePath))
                {
                    result = new StringCollection();

                    using (StreamReader sr = File.OpenText(indexFilePath))
                    {
                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                            result.Add(line);
                    }
                }
            }
            catch
            { }

            return result;
        }
    }
}
