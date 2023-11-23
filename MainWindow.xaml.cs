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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Asteroids
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }
        
        //TODO Startbereich soll frei von Asteroiden sein.
        //TODO Ein weiteres Fenster "Startmenü" für Startwerte bauen. Wird nach Spielende gewonnen oder verloren erneut aufgerufen
        //TODO Ein Spielobjekt UFO, welches Unruhe stiften soll.
        //TODO Wird ein Spieler von einem Geschoss getroffen soll er in die Richtung geschuppst werden und eine Rotation erfahren
        //TODO Keine Hitboxen, die ersten 2 Sekunden. Ballern am besten auch nicht möglich
        
        //Spieloptionen
        int AnzahlAsteroidenGroß = 6;
        int AnzahlSpieler = 1;
        // int FramesPerSeconds = 40;

        //Spielobjekte
        List<Raumschiff> raumschiffe = new List<Raumschiff>();
        List<Asteroid> asteroiden = new List<Asteroid>();
        List<Geschoss> geschosse = new List<Geschoss>();
        List<Spielobjekt> Spielobjekte = new List<Spielobjekt>();

        //Sonstige
        DispatcherTimer timer = new DispatcherTimer();

        private void zeichenfläche_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < AnzahlSpieler; i++)
            {
                raumschiffe.Add(new Raumschiff(zeichenfläche));
                Spielobjekte.Add(raumschiffe.Last());
            }

            for (int i = 0; i < AnzahlAsteroidenGroß; i++)
            {
                asteroiden.Add(new Asteroid(zeichenfläche, 3));
                Spielobjekte.Add(asteroiden.Last());
            }

            timer.Interval = TimeSpan.FromSeconds(0.025); //statt 0.025 auch 1/FramesperSeconds möglich (siehe Spieloptionen)
            timer.Tick += Animiere;
            timer.Start();            
        }

        private void Animiere(object sender, EventArgs e)
        {
            //Berechnung aller Bewegungen und Positionen aller Spielobjekte.
            foreach (var item in Spielobjekte)
            {
                item.Berechnen(zeichenfläche, timer.Interval);
            }

            //Kollisionsabfrage zwischen Asteroiden und Geschossen
            foreach (Asteroid a in asteroiden)
            {
                foreach (Geschoss g in geschosse)
                {
                    if(a.HitboxTriggert(g))
                    {
                        a.Zerstört = true;
                        g.Zerstört = true;
                    }
                }
            }

            //Kollisionsabfrage zwischen Asteroiden und Spielerraumschiffen
            foreach (Asteroid a in asteroiden)
            {
                foreach (Raumschiff r in raumschiffe)
                {
                    if (a.HitboxTriggert(r))
                    {
                        int aktuellerSpieler = raumschiffe.IndexOf(r) + 1;
                        MessageBox.Show("Krachbumm! Spieler " + aktuellerSpieler + " wurde zerstört!");
                        break;
                    }
                }
            }

            //neue kleinere Asteroiden konstruieren, wenn ein Asteroid von einem Geschoss getroffen wurde
            for (int i = 0; i < asteroiden.Count; i++) 
            {
                if (asteroiden[i].Zerstört && asteroiden[i].Größe > 1)
                {
                    for (int j = 0; j <= (4 - asteroiden[i].Größe); j++)
                    {
                        asteroiden.Add(new Asteroid(zeichenfläche, asteroiden[i].PositionX, asteroiden[i].PositionY, asteroiden[i].Größe - 1));
                        Spielobjekte.Add(asteroiden.Last());
                    }
                }
            }

            //Alle nicht zerstörten Spielobjekte, sowie neue Asteroiden mit ihrer aktuellen Position auf der Zeichenfläche anzeigen
            foreach (var item in Spielobjekte)
            {
                item.Darstellen(zeichenfläche);
            }

            //Zerstörte Asteroiden, sowie Geschosse mit abgelaufener Lebensspanne aus der Liste der Spielobjekten löschen.
            for (int i = 0; i < Spielobjekte.Count; i++) 
            {
                if (Spielobjekte[i].Zerstört)
                {
                    Spielobjekte.Remove(Spielobjekte[i]);
                }
            }

            //Zerstörte Asteroiden aus ihre eigenen Liste entfernen
            for (int i = 0; i < asteroiden.Count; i++)
            {
                if (asteroiden[i].Zerstört)
                {
                    asteroiden.Remove(asteroiden[i]);
                }
            }

            //Kollidierte Geschosse, sowie Geschosse mit abgelaufener Lebensdauer entfernen.
            for (int i = 0; i < geschosse.Count; i++)
            {
                if (geschosse[i].Zerstört)
                {
                    geschosse.Remove(geschosse[i]);
                }
            }

            //Siegbedingung
            if (asteroiden.Count == 0) 
            {
                MessageBox.Show("Gewonnen!");
                restartGame();
            }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Up:
                    raumschiffe[0].Thruster = true;
                    break;
                case Key.Left:
                    raumschiffe[0].TurnLeft = true;
                    break;
                case Key.Right:
                    raumschiffe[0].TurnRight = true;
                    break;
                case Key.RightCtrl:
                    {
                        geschosse.Add(new Geschoss(zeichenfläche, raumschiffe[0]));
                        Spielobjekte.Add(geschosse.Last());  //TODO Nur ein Torpedo je Tastendruck - Im Moment ist halten möglich
                        break;
                    }
            }
            
            if( AnzahlSpieler == 2)
            {
                switch (e.Key)
                {
                    case Key.W:
                        raumschiffe[1].Thruster = true;
                        break;
                    case Key.A:
                        raumschiffe[1].TurnLeft = true;
                        break;
                    case Key.D:
                        raumschiffe[1].TurnRight = true;
                        break;
                    case Key.LeftCtrl:
                        geschosse.Add(new Geschoss(zeichenfläche, raumschiffe[1]));
                        Spielobjekte.Add(geschosse.Last());  //TODO Nur ein Torpedo je Tastendruck - Im Moment ist halten möglich
                        break;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Up:
                    raumschiffe[0].Thruster = false;
                    break;
                case Key.Left:
                    raumschiffe[0].TurnLeft = false;
                    break;
                case Key.Right:
                    raumschiffe[0].TurnRight = false;
                    break;
            }

            if (AnzahlSpieler == 2)
            {
                switch (e.Key)
                {
                    case Key.W:
                        raumschiffe[1].Thruster = false;
                        break;
                    case Key.A:
                        raumschiffe[1].TurnLeft = false;
                        break;
                    case Key.D:
                        raumschiffe[1].TurnRight = false;
                        break;
                }
            }
        }
        private void restartGame()
        {
            for (int i = 0; i < AnzahlAsteroidenGroß ; i++)
            {
                asteroiden.Add(new Asteroid(zeichenfläche, 3));
                Spielobjekte.Add(asteroiden.Last());
            }
        }
    }
}
