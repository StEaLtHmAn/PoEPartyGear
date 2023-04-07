using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;

public class NewProgressBar : ProgressBar
{
    public NewProgressBar()
    {
        this.SetStyle(ControlStyles.UserPaint, true);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        //base.OnPaintBackground(pevent);
    }

    double DrawProgressValue = 0;
    protected override void OnPaint(PaintEventArgs e)
    {
        using (Image offscreenImage = new Bitmap(this.Width, this.Height))
        {
            using (Graphics offscreen = Graphics.FromImage(offscreenImage))
            {
                offscreen.FillRectangle(Brushes.LightGray, 0, 0, Width, Height);

                int rectWidth = (int)(Width * (DrawProgressValue / Maximum));
                Rectangle rect = new Rectangle(RightToLeftLayout ? Width - rectWidth : 1, 1, rectWidth == 0 ? 1 : rectWidth, Height);
                LinearGradientBrush brush = new LinearGradientBrush(rect, ControlPaint.Light(ForeColor), ForeColor, LinearGradientMode.Vertical);
                offscreen.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);

                e.Graphics.DrawImage(offscreenImage, 0, 0);
            }
        }
        if (DrawProgressValue < Value)
            DrawProgressValue += (Value - DrawProgressValue)*0.1;
        else if (DrawProgressValue > Value)
            DrawProgressValue--;
    }
}