using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MMIKinect.Network {
	/// <summary>
	/// Logique d'interaction pour ConnectionsInfo.xaml
	/// </summary>
	partial class ConnectionsInfo : Window {

		public ConnectionsInfo() {
			InitializeComponent();
		}

		public string ServerAdress {
			get { return _serverAdress.Text; }
			set { _serverAdress.Text = value; }
		}

		public int ServerPort {
			get { return int.Parse(_portServer.Text); }
			set { _portServer.Text = value.ToString(); }
		}

		private bool isValidInformations() {
			if(_serverAdress.Text.Length == 0) throw new Exception("Adresse Serveur Invalide !");
			try { int.Parse(_portServer.Text); } catch(Exception) { throw new Exception("Port Serveur Invalide !"); }
			return true;
		}

		private void OnClickConnection( object sender, RoutedEventArgs e ) {
			try {
				if(isValidInformations()) {
					this.DialogResult = true;
					Close();
				}
			} catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}



	}
}
