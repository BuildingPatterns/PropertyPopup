using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PropertyPopup
{
    public interface ICadComponent
    {
        #region Properties 
        Color Fillcolor { get; set; }                           //Fillcolor
        string Name { get; set; }                               //Name
        Color Pencolor { get; set; }                            //Pencolor
        int Penwidth { get; set; }                              //Penwidth
        Rectangle Rect { get; set; }                            //Rect
        #endregion

        bool OnItem(Point point);
        void Paint(PaintEventArgs e);
    }
    public class CadDraw : ICadComponent
    {
        #region Properties 
        //[Description("Background color of object")]             //Color
        public Color Color
        {
            get { return Face.BackColor; }
            set { Face.BackColor = value; }
        }

        PictureBox Face { get; set; }                           //Face

        public Color Fillcolor { get; set; }                    //Fillcolor

        [Description("Name of object")]                         //Name
        public string Name { get; set; }

        List<ICadComponent> Items { get; set; }                 //Items

        public Color Pencolor { get; set; }                     //Pencolor

        public int Penwidth { get; set; }                       //Penwidth

        //[Browsable(false)]                                      //Popupmenu
        public ContextMenuStrip Popupmenu { get; set; }         

        public Rectangle Rect { get; set; }                     //Rect
        #endregion

        public CadDraw(PictureBox aFace): base()                       
        {
            Face = aFace;
            Color = SystemColors.Window;                           
            Face.BackColor = Color;
            Face.Paint += doPaint;
            Name = "Draw";
            Items = new List<ICadComponent>();
        }

        public void CreateSomeObjects(int num)
        {
            ICadComponent component;
            Random random = new Random();
            bool willRect = true;
            for (int i = 0; i < num; i++)
            {
                Size si = new Size(random.Next(30, 300), random.Next(30, 300));
                Point P1 = new Point(random.Next(Face.Width- si.Width), random.Next(Face.Height- si.Height));
                Rectangle rect = new Rectangle(P1, si);
                if (willRect)
                {
                    component = new CadRectangle(this);
                    component.Name = "Rectangle" + i.ToString();
                }
                else
                {
                    component = new CadEllipse(this);
                    component.Name = "Ellipse" + i.ToString();
                }
                component.Rect = rect;
                component.Fillcolor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                component.Pencolor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                component.Penwidth = random.Next(1, 5);
                willRect = !willRect;
                Items.Add(component);
            }
            Invalidate();
        }
        private void doPaint(object sender, PaintEventArgs e)
        {
            foreach (ICadComponent comp in Items)
                comp.Paint(e);
        }
        public void Invalidate()
        {
            Face.Invalidate();
        }
        public void PopupClosing()
        {
            Invalidate();
        }
        public void PopupOpening()
        {
            Point mousePos = Face.PointToClient(Cursor.Position);
            ICadComponent selected = this;
            foreach(ICadComponent comp in Items)
            {
                if(comp.OnItem(mousePos))
                {
                    selected = comp;
                    break;
                }
            }
            Popupmenu.Items.Clear();
            new PropertyHandler(selected, Popupmenu.Items, 0);  //inserts property handler
        }

        #region ICadComponent
        public bool OnItem(Point point)             //dummy methode
        {
            return true;
        }
        public void Paint(PaintEventArgs e)         //dummy methode
        {
        }
        #endregion
    }
    public class CadEllipse: ICadComponent
    {
        #region Properties 
        CadDraw Draw { get; set; }                  //Draw

        //[Description("Fillcolor of object")]
        public Color Fillcolor { get; set; }        //Fillcolor

        [Description("Name of object")] 
        public string Name { get; set; }            //Name

        [Description("Rectangle of object")]
        public Rectangle Rect { get; set; }         //Rect

        //[Description("Pencolor of object")]
        public Color Pencolor { get; set; }         //Pencolor

        int penwidth = 1;
        [Description("Penwidth of object")]
        public int Penwidth                         //Penwidth
        {
            get
            {
                return penwidth;
            }
            set
            {
                penwidth = value;
                if (Draw != null)
                    Draw.Invalidate();
            }
        }
        #endregion

        public CadEllipse(CadDraw draw)
        {
            Draw = draw;
        }

        Region GetRgn()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(Rect);
            return new Region(path);
        }

        #region ICadComponent
        public bool OnItem(Point point)
        {
            return GetRgn().IsVisible(point);
        }
        public void Paint(PaintEventArgs e)                     //draws object on the drawface
        {
            e.Graphics.FillEllipse(new SolidBrush(Fillcolor), Rect);
            e.Graphics.DrawEllipse(new Pen(Pencolor, Penwidth), Rect);
        }
        #endregion
    }
    public class CadRectangle : ICadComponent
    {
        #region Properties 
        CadDraw Draw { get; set; }                  //Draw

        //[Description("Fillcolor of object")]
        public Color Fillcolor { get; set; }        //Fillcolor

        [Description("Name of object")] 
        public string Name { get; set; }            //Name

        [Description("Rectangle of object")]
        public Rectangle Rect { get; set; }         //Rect

        //[Description("Pencolor of object")]
        public Color Pencolor { get; set; }         //Pencolor

        int penwidth = 1;
        [Description("Penwidth of object")]
        public int Penwidth                         //Penwidth
        {
            get
            {
                return penwidth;
            }
            set
            {
                penwidth = value;
                if (Draw != null)
                    Draw.Invalidate();
            }
        }
        #endregion

        public CadRectangle(CadDraw draw)          
        {
            Draw = draw;
        }

        Region GetRgn()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(Rect);
            return new Region(path);
        }

        #region ICadComponent
        public bool OnItem(Point point)
        {
            return GetRgn().IsVisible(point);
        }
        public void Paint(PaintEventArgs e)                     //draws object on the drawface
        {
            e.Graphics.FillRectangle(new SolidBrush(Fillcolor), Rect);
            e.Graphics.DrawRectangle(new Pen(Pencolor, Penwidth), Rect);
        }
        #endregion
    }
}
