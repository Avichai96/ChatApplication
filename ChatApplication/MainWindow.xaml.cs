using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using ChatApplication.ChatService;
using Microsoft.Win32;

namespace ChatApplication
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IChatServiceCallback
	{
		private readonly ChatServiceClient _chatClient;
		private Configuration _configuration;
		private readonly string _configPath = AppDomain.CurrentDomain.BaseDirectory + "config.xml";

		public MainWindow()
		{
			InitializeComponent();

			_chatClient = new ChatServiceClient(new System.ServiceModel.InstanceContext(this), "ChatServiceEndpoint");
		}

		private async void SendButton_Click(object sender, RoutedEventArgs e)
		{
			// check message content
			if (!string.IsNullOrEmpty(MessageTextBox.Text))
			{
				try
				{
					// send the message
					await _chatClient?.SendMessageAsync(MessageTextBox.Text, _configuration.ClientName);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error sending the message\n\n" + ex.Message);
				}
			}
		}

		private async void FileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "Json or Xml files (*.json, *.xml) | *.json; *.xml;"
			};
			// show the file dialog and wait for result
			if (dialog.ShowDialog().Value)
			{
				string fileName = dialog.FileName;

				// make sure the file still exists
				if (File.Exists(fileName) == false)
				{
					MessageBox.Show("File does not exists");
					return;
				}

				FileInfo info = new FileInfo(fileName);
				try
				{
					// open a stream to the computer storage
					using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
					{
						// read the stream content
						using (StreamReader reader = new StreamReader(stream))
						{
							string content = await reader.ReadToEndAsync();

							// convert to byte array
							byte[] fileData = Encoding.Default.GetBytes(content);

							// send the file raw data to the server
							await _chatClient.SendFileAsync(fileData, info.Name, _configuration.ClientName);
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error sending file!\n\n" + ex.Message);
				}
			}
		}

		private void ReadConfig(string path)
		{
			// load default config
			_configuration = new Configuration
			{
				ClientName = "Client1"
			};

			// make sure the configuration file exists
			if (File.Exists(path))
			{
				try
				{
					// read the xml configuration file
					using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						// serialize the xml to the configuration field
						XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
						_configuration = (Configuration)serializer.Deserialize(stream);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error reading configuration file\n\n" + ex.Message);
				}
			}
		}

		private void WriteConfig(string path)
		{
			try
			{
				// open a stream to the storage
				using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
				{
					// serialize the configuration field as xml
					XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

					// save the xml to the stream
					serializer.Serialize(stream, _configuration);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error writing the configuration file\n\n" + ex.Message);
			}
		}

		private void This_Loaded(object sender, RoutedEventArgs e)
		{
			ReadConfig(_configPath);
			ClientTextBlock.Text = _configuration.ClientName;

			_chatClient.Open();
			_chatClient.RegisterAsync(_configuration.ClientName);
		}

		private void This_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (_chatClient.State != System.ServiceModel.CommunicationState.Faulted)
			{
				_chatClient.UnRegisterAsync(_configuration.ClientName); 
			}
		}

		private void This_Closed(object sender, EventArgs e)
		{
			WriteConfig(_configPath);
			_chatClient.Close();
		}


		#region Callback implementation

		public void MessageReceived(string message, string clientName)
		{
			// create and show the message item
			string name = clientName != _configuration.ClientName ? clientName : "you";
			var item = new ListViewItem
			{
				Content = $"{name}: {message}"
			};
			Messages.Items.Add(item);
		}

		public void FileReceived(byte[] file, string fileName, string clientName)
		{
			// seperate the file extesion from the file name
			string extension = fileName.Split('.').Last();

			// convert the raw data to text
			string fileString = Encoding.Default.GetString(file);

			// create a view item
			string header = $"{clientName} sent a {extension} file. Double click to view";
			var item = new ListViewItem()
			{
				Content = header,
			};
			item.MouseDoubleClick += (sender, args) =>
			{
				// create and show a file viewer window
				FileWindow fileWindow = new FileWindow
				{
					Item = fileString
				};
				fileWindow.Show();
			};

			// view the item
			Messages.Items.Add(item);
		}

		public void ClientRegistered(string clientName)
		{
			// create and show the message item
			string header = $"{clientName} has joined the group";
			if (clientName == _configuration.ClientName)
			{
				header = "you have joined the group";
			}
			var item = new ListViewItem
			{
				Content = header
			};
			Messages.Items.Add(item);
		}

		public void ClientUnRegistered(string clientName)
		{
			// create and show the message item
			var item = new ListViewItem
			{
				Content = $"{clientName} has left the group"
			};
			Messages.Items.Add(item);
		} 

		#endregion
	}
}
