namespace Eresys.Extra
{
    public static class Utility
    {
        /// <summary>
        /// Een constructor die een texture aanmaakt met een meegegeven WAD3 en een naam.
        /// </summary>
        /// <param name="wad">De WAD3 waaruit de texture moet geladen worden.</param>
        /// <param name="name">De naam van de texture die geladen moet worden.</param>
        public static Texture ToTexture(this Wad3 wad, string name)
        {
            // Variables used for loading
            int length; byte[] bytes;

            Wad3Lump lump = wad.GetLump(name);

            var texture = new Texture(lump.Width, lump.Height, name);

            // Set length of texture in bytes (8-bit format) & colors
            length = texture.Width * texture.Height;

            // Get the bytes for mip level 0 (1) from wad
            bytes = wad.GetBytes(lump.Offset + lump.Offsets[0], length);

            // Create the color pallet
            Color[] pallet = new Color[256];
            byte[] palletBytes = wad.GetBytes(lump.Offset + lump.Offsets[3] + (length / 64) + 2, 768);

            // Fill the color pallet
            for (int i = 0, j = 0; i < 256; i++, j += 3)
                pallet[i] = new Color(palletBytes[j], palletBytes[j + 1], palletBytes[j + 2]);

            // check transparancy
            bool tranparant = name[0] == '{';

            //// Create the color array
            //texture.Data = new Color[length];
            //// Fill the actual data array with the correct (?) values
            //for (int i = 0; i < length; i++)
            //{
            //    Color pixel = pallet[bytes[i]];
            //    if (tranparant && pixel.ToColorCode() == (int)ColorKey)
            //        pixel = new Color();
            //    texture.data[i] = pixel;
            //}

            return texture;
        }
    }
}
