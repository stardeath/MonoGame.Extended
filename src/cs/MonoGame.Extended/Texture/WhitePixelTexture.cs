using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Texture
{
	public static class GraphicsDeviceWhitePixelTextureExtension
	{
		private static Texture2D s_Instance;

		public static Texture2D GetWhitePixelTexture( this GraphicsDevice device )
		{
			if( s_Instance == null )
			{
				s_Instance = new Texture2D( device, 1, 1 );
				s_Instance.SetData( new[] { Color.White } );
				AutoDispose( device );
			}
			return s_Instance;
		}

		private static void AutoDispose( GraphicsDevice device )
		{
			device.Disposing += ( sender, e ) =>
			{
				s_Instance?.Dispose();
				s_Instance = null;
			};
		}
	}
}
