using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Asteroids
{
    abstract class Spielobjekt
    {
        private double positionX;
        private double positionY;
        private double geschwindigkeitX;
        private double geschwindigkeitY;
        private double winkel;
        private double rotationsgeschwindigkeit;
        private bool zertört = false;

        protected Spielobjekt(double x, double y) //Raumschiff
        {
            this.positionX = x;
            this.positionY = y;
        }

        protected Spielobjekt(double x, double y, double w) //Geschoss
        {
            this.positionX = x;
            this.positionY = y;
            this.winkel = w;
        }

        protected Spielobjekt(double x, double y, double dX, double dY, double rotationsgeschwindigkeit) //Asteroid
        {
            this.positionX = x;
            this.positionY = y;
            this.geschwindigkeitX = dX;
            this.geschwindigkeitY = dY;
            this.rotationsgeschwindigkeit = rotationsgeschwindigkeit;
        }
        public double PositionX
        {
            get { return this.positionX; }
            set { this.positionX = value; }
        }

        public double PositionY
        {
            get { return this.positionY; }
            set { this.positionY = value; }
        }

        public double GeschwindigkeitX 
        {
            get { return this.geschwindigkeitX; }
            set { this.geschwindigkeitX = value;}
        }

        public double GeschwindigkeitY
        {
            get { return this.geschwindigkeitY; }
            set { this.geschwindigkeitY = value;}
        }

        public double Winkel
        {
            get { return this.winkel; }
            set { this.winkel = value; }
        }

        public bool Zerstört
        {
            get { return this.zertört; }
            set { this.zertört = value; }
        }

        protected double Rotationsgeschwindigkeit
        {
            get { return this.rotationsgeschwindigkeit; }
            set { this.rotationsgeschwindigkeit = value; }
        }
        public abstract void Konstruiere(Canvas zeichenfläche); //Bauen, Konstruieren, Startwerte im Konstruktor des weweiligen Objekts

        public abstract void Berechnen(Canvas zeichenfläche, TimeSpan periodischeDauer); //Positionen und Rotationen aller Spielobjekte berechnen

        public abstract void Darstellen(Canvas zeichenfläche); //Objekt in der Canvas anzeigen, bzw aus der zeichenfläche entfernen

        //public abstract void ZerstörteElementeEntfernen(Canvas zeichenfläche); //In der Main aufrufen. Notwenigkeit nicht da!
    }

    class Asteroid : Spielobjekt
    {
        private static Random zufall = new Random();
        private Polygon form;
        private int größe;  // Init = 3 (main)

        public int Größe
        {
            get { return this.größe; }
            set { this.größe = value; }
        }

        public Asteroid(Canvas zeichenfläche, int size) :   base(zeichenfläche.ActualWidth * zufall.NextDouble(),                       //Aufruf bei Spielbegin                           
                                                            zeichenfläche.ActualHeight * zufall.NextDouble(),
                    /*   Hier Geschwindigkeit einstellen */ zufall.Next(50 - size * 10) * (zufall.NextDouble() - 0.5),
                    /*              zufall.Next(min,max) */ zufall.Next(50 - size * 10) * (zufall.NextDouble() - 0.5),
                                                            zufall.Next(0, 25) * (zufall.NextDouble() - 0.5))
        {
            this.größe = size;
            Konstruiere(zeichenfläche); 
        }

        public Asteroid(Canvas zeichenfläche, double X, double Y, int size ) :  base(X, Y,                                              //Aufruf, wenn ein Asteroid zerstört wurde, Übernimmt die Position des zerstöten Asteroiden 
                                        /*   Hier Geschwindigkeit einstellen */ zufall.Next(50 - size*10) * (zufall.NextDouble() - 0.5),
                                        /*                  zufall.Next(max) */ zufall.Next(50 - size*10) * (zufall.NextDouble() - 0.5),
                                                                                zufall.Next(0, 25) * (zufall.NextDouble() - 0.5))
        {
            this.größe = size;
            Konstruiere(zeichenfläche);
        }

        public override void Konstruiere(Canvas zeichenfläche)
        {
            int AnzahlEcken = 10;                           // Man kann easy beliebig-Eckige Asteroiden generieren
            double Radius = 12 * Größe ;                    // Dargestellter Asteroid ist doppelter Wert in Pixel; Ist Radius
            double Bogenmaß = 2 * Math.PI / AnzahlEcken;    // Der Faktor i für Aktuelle-Ecke fehlt noch im Zähler (siehe for-Schleife)

            form = new Polygon();
            for (int i = 0; i < AnzahlEcken; i++) 
            {
                double Störfaktor = (1 + (zufall.NextDouble() - 0.5) * 0.3); //Variert den Radius und macht den Asteroiden "Klumbiger" -->  1 +- (Variation) ; 0.3 ist hier der Abweichungsfaktor
                form.Points.Add(new System.Windows.Point(Radius * Störfaktor * Math.Cos(i * Bogenmaß), Radius * Störfaktor * Math.Sin(i * Bogenmaß)));
            }
            //form.RenderTransformOrigin = new System.Windows.Point(0, 0);
            form.Fill = Brushes.Gray;
            zeichenfläche.Children.Add(form);
            Canvas.SetLeft(form, PositionX);
            Canvas.SetTop(form, PositionY);
        }

        public override void Berechnen(Canvas zeichenfläche, TimeSpan periodischeDauer) //Positionen und Rotationen aller Asteroiden berechnen
        {

            //Neue Position berechnen
            PositionX += GeschwindigkeitX * (periodischeDauer.TotalMilliseconds / 50);
            PositionY += GeschwindigkeitY * (periodischeDauer.TotalMilliseconds / 50);

            Winkel += Rotationsgeschwindigkeit * (periodischeDauer.TotalMilliseconds / 50); //Reine Optik, unwichtig für das eigentliche Spiel

            //Wenn Objekte aus dem Fenster fliegen, setze sie auf die andere Seite.
            //TODO Dont repeat yourself;
            //TODO Periodische Fortsetzung als boolischen Ausdrück für Horizontal und Vertikal
            if (PositionX > zeichenfläche.ActualWidth)
            { PositionX = 0; }
            else if (PositionX < 0)
            { PositionX = zeichenfläche.ActualWidth; }
            else if (PositionY > zeichenfläche.ActualHeight)
            { PositionY = 0; }
            else if (PositionY < 0)
            { PositionY = zeichenfläche.ActualHeight; }

            form.RenderTransform = new RotateTransform(Winkel-90);
        }

        public bool HitboxTriggert(Spielobjekt spielobjekt) //Ist der Mittelpunkt des Spielobekts innerhalb des Asteroiden 
        {
            return form.RenderedGeometry.FillContains(new System.Windows.Point(spielobjekt.PositionX - PositionX, spielobjekt.PositionY - PositionY));
        }

        public override void Darstellen(Canvas zeichenfläche) //Objekt in der Canvas anzeigen, bzw aus der Zeichenfläche entfernen
        {
            if (!Zerstört)
            {
                Canvas.SetLeft(form, PositionX);
                Canvas.SetTop(form, PositionY);
            }
            else
                zeichenfläche.Children.Remove(form);
        }
    }

    class Raumschiff : Spielobjekt
    {
        private bool thruster = false;
        private bool turnLeft = false;
        private bool turnRight = false;
        private Polygon form;

        public bool Thruster { get => thruster; set => thruster = value; }
        public bool TurnLeft { get => turnLeft; set => turnLeft = value; }
        public bool TurnRight { get => turnRight; set => turnRight = value; }

        public Raumschiff(Canvas zeichenfläche) : base( zeichenfläche.ActualWidth / 2, zeichenfläche.ActualHeight / 2)
        {
            Konstruiere(zeichenfläche);
        }

        public override void Konstruiere(Canvas zeichenfläche)
        {
            form = new Polygon();
            form.Points.Add(new System.Windows.Point(0, -18));
            form.Points.Add(new System.Windows.Point(3, 4));
            form.Points.Add(new System.Windows.Point(7, 4));
            form.Points.Add(new System.Windows.Point(10, -9));
            form.Points.Add(new System.Windows.Point(10, 9));
            form.Points.Add(new System.Windows.Point(2, 9));
            form.Points.Add(new System.Windows.Point(0, 12));
            form.Points.Add(new System.Windows.Point(-2, 9));
            form.Points.Add(new System.Windows.Point(-10, 9));
            form.Points.Add(new System.Windows.Point(-10, -9));
            form.Points.Add(new System.Windows.Point(-7, 4));
            form.Points.Add(new System.Windows.Point(-3, 4));
            form.Fill = Brushes.Cyan;

            //form.RenderTransformOrigin = new System.Windows.Point(0, -0.3);
            zeichenfläche.Children.Add(form);

            Canvas.SetLeft(form, PositionX);
            Canvas.SetTop(form, PositionY);
        }

        public override void Berechnen(Canvas zeichenfläche, TimeSpan periodischeDauer)
        {
            if (thruster == true)
                Beschleunigen(zeichenfläche, periodischeDauer);

            if (turnLeft == true)
                DreheSchiff(false);

            if (turnRight == true)
                DreheSchiff(true);

            if (turnLeft == false && turnRight == false)
                Rotationsgeschwindigkeit = 0;
            else
                Winkel += Rotationsgeschwindigkeit * (periodischeDauer.TotalMilliseconds / 50);

            //Neue Position berechnen
            PositionX += GeschwindigkeitX * (periodischeDauer.TotalMilliseconds / 50);
            PositionY += GeschwindigkeitY * (periodischeDauer.TotalMilliseconds / 50);

            //Wenn Objekte aus dem Fenster fliegen, setze sie auf die andere Seite.
            //TODO Dont repeat yourself;
            //TODO Periodische Fortsetzung als boolischen Ausdrück für Horizontal und Vertikal
            if (PositionX > zeichenfläche.ActualWidth)
            { PositionX = 0; }
            else if (PositionX < 0)
            { PositionX = zeichenfläche.ActualWidth; }
            else if (PositionY > zeichenfläche.ActualHeight)
            { PositionY = 0; }
            else if (PositionY < 0)
            { PositionY = zeichenfläche.ActualHeight; }

            //Trägheit simulieren. Abnahme der Geschwindigkeit.
            GeschwindigkeitX *= 0.98;
            GeschwindigkeitY *= 0.98;

            form.RenderTransform = new RotateTransform(Winkel + 90);
        }

        public void Beschleunigen(Canvas zeichenfläche, TimeSpan periodischeDauer)
        {
            //berechne mit cos und sin den normierten Wert für dx und dy
            GeschwindigkeitX = GeschwindigkeitX + (Math.Cos(Winkel * 2 * Math.PI / 360) * (periodischeDauer.TotalMilliseconds / 50)); //TODO Beschleuigung Framesabhägigkeit
            GeschwindigkeitY = GeschwindigkeitY + (Math.Sin(Winkel * 2 * Math.PI / 360) * (periodischeDauer.TotalMilliseconds / 50));
        }

        public void DreheSchiff(bool Richtung)
        {
            Rotationsgeschwindigkeit = Richtung ? 8 : -8;
        }

        public override void Darstellen(Canvas zeichenfläche)
        {
            Canvas.SetLeft(form, PositionX);
            Canvas.SetTop(form, PositionY);
        }
    }

    class Geschoss : Spielobjekt
    {
        double alter = 0;
        private int lebensspanne = 14; //objekt soll nach einer einer gewissen Zeit von alleine sterben
        private double sollPewPewGeschwindigkeit = 15;

        private Ellipse pew;

        public Geschoss(Canvas zeichenfläche, Raumschiff Spieler) : base(Spieler.PositionX, Spieler.PositionY, Spieler.Winkel) 
        {
            //Geschwindigkeitsfaktor = sollPewPewGeschwindigkeit / Math.Sqrt(Math.Cos(Winkel * 2 * Math.PI / 360) * Math.Cos(Winkel * 2 * Math.PI / 360) + Math.Sin(Winkel * 2 * Math.PI / 360) * Math.Sin(Winkel * 2 * Math.PI / 360));

            //GeschwindigkeitX = Spieler.GeschwindigkeitX + (Geschwindigkeitsfaktor * Math.Cos(Winkel * 2 * Math.PI / 360));
            //GeschwindigkeitY = Spieler.GeschwindigkeitY + (Geschwindigkeitsfaktor * Math.Sin(Winkel * 2 * Math.PI / 360));

            //Zu beweisen gilt. Sind die nächsten 2 Zeilen == die 3 obeneren Zeilen
            GeschwindigkeitX = Spieler.GeschwindigkeitX + (sollPewPewGeschwindigkeit * Math.Cos(Winkel * 2 * Math.PI / 360));
            GeschwindigkeitY = Spieler.GeschwindigkeitY + (sollPewPewGeschwindigkeit * Math.Sin(Winkel * 2 * Math.PI / 360));

            Konstruiere(zeichenfläche);
        }

        public override void Konstruiere(Canvas zeichenfläche)
        {
            pew = new Ellipse();
            pew.Height = 5;
            pew.Width = 5;
            pew.Fill = Brushes.Red;
            zeichenfläche.Children.Add(pew);
            Canvas.SetLeft(pew, PositionX);
            Canvas.SetTop(pew, PositionY);
        }

        public override void Berechnen(Canvas zeichenfläche, TimeSpan periodischeDauer)
        {
            PositionX += (GeschwindigkeitX * (periodischeDauer.TotalMilliseconds / 50));
            PositionY += (GeschwindigkeitY * (periodischeDauer.TotalMilliseconds / 50));

            //Wenn Objekte aus dem Fenster fliegen, setze sie auf die andere Seite.
            //TODO Dont repeat yourself;
            //TODO Periodische Fortsetzung als boolischen Ausdrück für Horizontal und Vertikal
            if (PositionX > zeichenfläche.ActualWidth)
            { PositionX = 0; }
            else if (PositionX < 0)
            { PositionX = zeichenfläche.ActualWidth; }
            else if (PositionY > zeichenfläche.ActualHeight)
            { PositionY = 0; }
            else if (PositionY < 0)
            { PositionY = zeichenfläche.ActualHeight; }

            alter = alter + (periodischeDauer.TotalMilliseconds / 50);
        }

        public override void Darstellen(Canvas zeichenfläche)
        {
            if (!Zerstört && alter < lebensspanne)
            {
                Canvas.SetLeft(pew, PositionX);
                Canvas.SetTop(pew, PositionY);
            }
            else
            {
                Zerstört = true;
                zeichenfläche.Children.Remove(pew);
            }
        }
    }
}
