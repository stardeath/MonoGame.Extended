using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.Extended.Graphics
{
	public sealed partial class Batcher2D : Batcher<Batcher2D.DrawCallInfo>
	{
		/// <summary>
		///     Draws unicode (UTF-16) characters as sprites using the specified <see cref="BitmapFont" />, text
		///     <see cref="StringBuilder" />, transform <see cref="Matrix2" /> and optional <see cref="Color" />, origin
		///     <see cref="Vector2" />, <see cref="FlipFlags" />, and depth <see cref="float" />.
		/// </summary>
		/// <param name="bitmapFont">The <see cref="BitmapFont" />.</param>
		/// <param name="text">The text <see cref="StringBuilder" />.</param>
		/// <param name="transformMatrix">The transform <see cref="Matrix2" />.</param>
		/// <param name="color">
		///     The <see cref="Color" />. Use <code>null</code> to use the default
		///     <see cref="Color.White" />.
		/// </param>
		/// <param name="flags">The <see cref="FlipFlags" />. The default value is <see cref="FlipFlags.None" />.</param>
		/// <param name="depth">The depth <see cref="float" />. The default value is <code>0f</code>.</param>
		/// <exception cref="InvalidOperationException">The <see cref="Batcher{TDrawCallInfo}.Begin(ref Matrix, ref Matrix, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect)" /> method has not been called.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="bitmapFont" /> is null or <paramref name="text" /> is null.</exception>
		public void DrawString
		(
			BitmapFont bitmapFont,
			StringBuilder text,
			ref Matrix2 transformMatrix,
			Color? color = null,
			FlipFlags flags = FlipFlags.None,
			float depth = 0f
		)
		{
			EnsureHasBegun();

			if( bitmapFont == null )
				throw new ArgumentNullException( nameof( bitmapFont ) );

			if( text == null )
				throw new ArgumentNullException( nameof( text ) );

			var lineSpacing = bitmapFont.LineHeight;
			var offset = new Vector2( 0, 0 );

			BitmapFontRegion lastGlyph = null;
			for( var i = 0; i < text.Length; )
			{
				int character;
				if( char.IsLowSurrogate( text[i] ) )
				{
					character = char.ConvertToUtf32( text[i - 1], text[i] );
					i += 2;
				}
				else if( char.IsHighSurrogate( text[i] ) )
				{
					character = char.ConvertToUtf32( text[i], text[i - 1] );
					i += 2;
				}
				else
				{
					character = text[i];
					i += 1;
				}

				// ReSharper disable once SwitchStatementMissingSomeCases
				switch( character )
				{
					case '\r':
						continue;
					case '\n':
						offset.X = 0;
						offset.Y += lineSpacing;
						lastGlyph = null;
						continue;
				}

				var fontRegion = bitmapFont.GetCharacterRegion( character );
				if( fontRegion == null )
					continue;

				var transform1Matrix = transformMatrix;
				transform1Matrix.M31 += offset.X + fontRegion.XOffset;
				transform1Matrix.M32 += offset.Y + fontRegion.YOffset;

				var textureRegion = fontRegion.TextureRegion;
				var bounds = textureRegion.Bounds;
				DrawSprite( textureRegion.Texture, ref transform1Matrix, ref bounds, color, flags, depth );

				var advance = fontRegion.XAdvance + bitmapFont.LetterSpacing;
				if( BitmapFont.UseKernings && lastGlyph != null )
				{
					int amount;
					if( lastGlyph.Kernings.TryGetValue( character, out amount ) )
					{
						advance += amount;
					}
				}

				offset.X += i != text.Length - 1
					? advance
					: fontRegion.XOffset + fontRegion.Width;

				lastGlyph = fontRegion;
			}
		}

		/// <summary>
		///     Draws unicode (UTF-16) characters as sprites using the specified <see cref="BitmapFont" />, text
		///     <see cref="StringBuilder" />, position <see cref="Vector2" /> and optional <see cref="Color" />, rotation
		///     <see cref="float" />, origin <see cref="Vector2" />, scale <see cref="Vector2" /> <see cref="FlipFlags" />, and
		///     depth <see cref="float" />.
		/// </summary>
		/// <param name="bitmapFont">The <see cref="BitmapFont" />.</param>
		/// <param name="text">The text <see cref="string" />.</param>
		/// <param name="position">The position <see cref="Vector2" />.</param>
		/// <param name="color">
		///     The <see cref="Color" />. Use <code>null</code> to use the default
		///     <see cref="Color.White" />.
		/// </param>
		/// <param name="rotation">
		///     The angle <see cref="float" /> (in radians) to rotate each sprite about its <paramref name="origin" />. The default
		///     value is <code>0f</code>.
		/// </param>
		/// <param name="origin">
		///     The origin <see cref="Vector2" />. Use <code>null</code> to use the default
		///     <see cref="Vector2.Zero" />.
		/// </param>
		/// <param name="scale">
		///     The scale <see cref="Vector2" />. Use <code>null</code> to use the default
		///     <see cref="Vector2.One" />.
		/// </param>
		/// <param name="flags">The <see cref="FlipFlags" />. The default value is <see cref="FlipFlags.None" />.</param>
		/// <param name="depth">The depth <see cref="float" />. The default value is <code>0f</code></param>
		/// <exception cref="InvalidOperationException">The <see cref="Batcher{TDrawCallInfo}.Begin(ref Matrix, ref Matrix, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect)" /> method has not been called.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="bitmapFont" /> is null or <paramref name="text" /> is null.</exception>
		public void DrawString
		(
			BitmapFont bitmapFont,
			StringBuilder text,
			Vector2 position,
			Color? color = null,
			float rotation = 0f,
			Vector2? origin = null,
			Vector2? scale = null,
			FlipFlags flags = FlipFlags.None,
			float depth = 0f
		)
		{
			Matrix2 transformMatrix;
			Matrix2.CreateFrom( position, rotation, scale, origin, out transformMatrix );
			DrawString( bitmapFont, text, ref transformMatrix, color, flags, depth );
		}

		/// <summary>
		///     Draws unicode (UTF-16) characters as sprites using the specified <see cref="BitmapFont" />, text
		///     <see cref="string" />, transform <see cref="Matrix2" /> and optional <see cref="Color" />, origin
		///     <see cref="Vector2" />, <see cref="FlipFlags" />, and depth <see cref="float" />.
		/// </summary>
		/// <param name="bitmapFont">The <see cref="BitmapFont" />.</param>
		/// <param name="text">The text <see cref="string" />.</param>
		/// <param name="transformMatrix">The transform <see cref="Matrix2" />.</param>
		/// <param name="color">
		///     The <see cref="Color" />. Use <code>null</code> to use the default
		///     <see cref="Color.White" />.
		/// </param>
		/// <param name="flags">The <see cref="FlipFlags" />. The default value is <see cref="FlipFlags.None" />.</param>
		/// <param name="depth">The depth <see cref="float" />. The default value is <code>0f</code></param>
		/// <exception cref="InvalidOperationException">The <see cref="Batcher{TDrawCallInfo}.Begin(ref Matrix, ref Matrix, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect)" /> method has not been called.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="bitmapFont" /> is null or <paramref name="text" /> is null.</exception>
		public void DrawString
		(
			BitmapFont bitmapFont,
			string text,
			ref Matrix2 transformMatrix,
			Color? color = null,
			FlipFlags flags = FlipFlags.None,
			float depth = 0f
		)
		{
			EnsureHasBegun();

			if( bitmapFont == null )
				throw new ArgumentNullException( nameof( bitmapFont ) );

			if( text == null )
				throw new ArgumentNullException( nameof( text ) );

			var glyphs = bitmapFont.GetGlyphs( text );
			foreach( var glyph in glyphs )
			{
				var transform1Matrix = transformMatrix;
				transform1Matrix.M31 += glyph.Position.X;
				transform1Matrix.M32 += glyph.Position.Y;

				var texture = glyph.FontRegion.TextureRegion.Texture;
				var bounds = texture.Bounds;
				DrawSprite( texture, ref transform1Matrix, ref bounds, color, flags, depth );
			}
		}

		/// <summary>
		///     Draws unicode (UTF-16) characters as sprites using the specified <see cref="BitmapFont" />, text
		///     <see cref="string" />, position <see cref="Vector2" /> and optional <see cref="Color" />, rotation
		///     <see cref="float" />, origin <see cref="Vector2" />, scale <see cref="Vector2" /> <see cref="FlipFlags" />, and
		///     depth <see cref="float" />.
		/// </summary>
		/// <param name="bitmapFont">The <see cref="BitmapFont" />.</param>
		/// <param name="text">The text <see cref="string" />.</param>
		/// <param name="position">The position <see cref="Vector2" />.</param>
		/// <param name="color">
		///     The <see cref="Color" />. Use <code>null</code> to use the default
		///     <see cref="Color.White" />.
		/// </param>
		/// <param name="rotation">
		///     The angle <see cref="float" /> (in radians) to rotate each sprite about its <paramref name="origin" />. The default
		///     value is <code>0f</code>.
		/// </param>
		/// <param name="origin">
		///     The origin <see cref="Vector2" />. Use <code>null</code> to use the default
		///     <see cref="Vector2.Zero" />.
		/// </param>
		/// <param name="scale">
		///     The scale <see cref="Vector2" />. Use <code>null</code> to use the default
		///     <see cref="Vector2.One" />.
		/// </param>
		/// <param name="flags">The <see cref="FlipFlags" />. The default value is <see cref="FlipFlags.None" />.</param>
		/// <param name="depth">The depth <see cref="float" />. The default value is <code>0f</code></param>
		/// <exception cref="InvalidOperationException">The <see cref="Batcher{TDrawCallInfo}.Begin(ref Matrix, ref Matrix, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect)" /> method has not been called.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="bitmapFont" /> is null or <paramref name="text" /> is null.</exception>
		public void DrawString
		(
			BitmapFont bitmapFont,
			string text,
			Vector2 position,
			Color? color = null,
			float rotation = 0f,
			Vector2? origin = null,
			Vector2? scale = null,
			FlipFlags flags = FlipFlags.None,
			float depth = 0f
		)
		{
			Matrix2 matrix;
			Matrix2.CreateFrom( position, rotation, scale, origin, out matrix );
			DrawString( bitmapFont, text, ref matrix, color, flags, depth );
		}
	}
}
