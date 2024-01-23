using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PhotoSorter
{
    public partial class PhotoSorterGrandad : Form
    {
        string path = "";
        string fileType;
        string oldPath;
        string newPath;
        string[] files;
        int index = 0;
        Bitmap bmp;
        DialogResult dialogResult;
        List<string> photos = new List<string>();

        public PhotoSorterGrandad()
        {
            InitializeComponent();
            Width = 800;
            Height = 680;
            BackgroundImageLayout = ImageLayout.Zoom;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            Shown += FormShown;
        }

        private void FormShown(object sender, EventArgs e)
        {
            MessageBox.Show("Press G for photos of Grandad\nPress F for photos not of Grandad\nPress Escape to Close", "Controls");
            do
            {
                do GetPath();
                while (NoPath());
            }
            while (!GetFiles());
            KeyUp += KeyUpEvent;
            bmp = new Bitmap(photos[index]);
            BackgroundImage = bmp;
        }
        /// <summary>
        /// Selects a path and if the subdirectories do not exist, creates them. 
        /// </summary>
        private void GetPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            path = fbd.SelectedPath;
            if (!Directory.Exists(path + "\\Grandad")) Directory.CreateDirectory(path + "\\Grandad");
            if (!Directory.Exists(path + "\\Not Grandad")) Directory.CreateDirectory(path + "\\Not Grandad");
        }

        /// <summary>
        /// No Path checks whether a path is set for the images to be sorted from
        /// </summary>
        /// <returns>True if there is no path set (empty string)</returns>
        private bool NoPath()
        {
            if (path == "") MessageBox.Show("Path Needed");
            return path == "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>GetFiles returns true if the directory has files, else asks the user if they want to select another directory</returns>
        private bool GetFiles()
        {
            files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                fileType = file.Split('.').Last();
                if (fileType.ToUpper() == "PNG" || fileType.ToUpper() == "JPG")
                    photos.Add(file);
            }
            if (photos.Count == 0)
            {
                dialogResult = MessageBox.Show("The directory selected has no photos of type JPG or PNG\nDo you wish to select another", "No Photos in Directory",MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    Application.Exit();
                    Close();
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method for handling Keyboard controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Escape)) Close();
            if (e.KeyCode.Equals(Keys.G)) MovePhoto(true);
            if (e.KeyCode.Equals(Keys.F)) MovePhoto(false);
            if (e.KeyCode.Equals(Keys.Z)) UndoRedo();
        }

        /// <summary>
        /// Moves the photo to the corresponding directory, renaming the file with (i) after the name if the filename already exists in that directory
        /// </summary>
        /// <param name="grandad">Sepcifies whether to move the file to the "grandad" folder or the "not grandad" folder</param>
        private void MovePhoto(bool grandad)
        {
            // Select current path of photo and create destination path
            oldPath = photos[index];
            newPath = path + (grandad ? "\\Grandad\\" : "\\Not Grandad\\") + photos[index].Split('\\').Last();
            BackgroundImage.Dispose();
            int i = 2;
            // If new file path is already in use, iterate until an available new file path is selected and move the file
            while (File.Exists(oldPath) && File.Exists(newPath))
            {
                newPath = path + (grandad ? "\\Grandad\\" : "\\Not Grandad\\")
                    + photos[index].Split('\\').Last().Split('.')[0] + " (" + i++ + ")."
                    + photos[index].Split('\\').Last().Split('.')[1];
            }
            File.Move(oldPath, newPath);
            // Select next photo
            index++;
            SetPhoto();
        }

        /// <summary>
        /// Undo or redo the moving of the last image file
        /// </summary>
        private void UndoRedo()
        {
            // If a photo has been moved before
            if (oldPath != null)
            {
                // Swap old and new paths (needed for redo) and move the image file
                string temp = oldPath;
                oldPath = newPath;
                newPath = temp;
                BackgroundImage.Dispose();
                File.Move(oldPath, newPath);
                if (photos.IndexOf(oldPath) > -1)
                    index++; // Increase if redo
                else
                    index--; // Decrease if undo
                SetPhoto();
            }
        }

        /// <summary>
        /// Sets the background of the form to the next photo to sort.
        /// If the last photo sorted was the last photo to sort,
        /// confirm that it was sorted. If correct then close the application,
        /// otherwise undo the image file movement.
        /// </summary>
        private void SetPhoto()
        {
            BackgroundImage.Dispose();
            if (index == photos.Count)
            {
                dialogResult = MessageBox.Show("Confirm last photo was sorted correctly.", "Please Confirm", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) Close();
                else UndoRedo();
                return;
            }
            bmp = new Bitmap(photos[index]);
            BackgroundImage = bmp;
        }
    }
}
