using System;

namespace Eresys
{
	/// <summary>
	/// Deze struct beschrijft een 2dimensioneel punt.
	/// </summary>
	public struct Point2D
	{
		/// <summary>
		/// Variabelen x , y (de co�rdinaten)
		/// </summary>
		public float x, y;

		/// <summary>
		/// De constructor die de co�rds meekrijgt
		/// </summary>
		/// <param name="x">Geef de x co�rdinaat mee.</param>
		/// <param name="y">Geef de y co�rdinaat mee.</param>
		public Point2D(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}
}
