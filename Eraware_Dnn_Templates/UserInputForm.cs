using System.Drawing;
using System.Windows.Forms;

namespace Eraware_Dnn_Templates
{
    public partial class UserInputForm : Form
    {
        private static string rootNamespace;
        private TextBox namespaceTextBox;
        private Button createButton;

        public UserInputForm()
        {
            this.Size = new Size(155, 265);

            createButton = new Button();
            createButton.Location = new Point(90, 25);
            createButton.Size = new Size(50, 25);
            createButton.Click += CreateButton_Click;
            this.Controls.Add(createButton);

            namespaceTextBox = new TextBox();
            namespaceTextBox.Location = new Point(10, 25);
            namespaceTextBox.Size = new Size(70, 20);
            this.Controls.Add(namespaceTextBox);
        }

        private void CreateButton_Click(object sender, System.EventArgs e)
        {
            rootNamespace = namespaceTextBox.Text;
            this.Close();
        }

        public static string RootNamespace {
            get
            {
                return rootNamespace;
            }
            set
            {
                rootNamespace = value;
            }
        }

        
    }
}