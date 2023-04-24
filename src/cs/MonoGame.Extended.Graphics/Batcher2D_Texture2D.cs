using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
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
		///     Draws a sprite using a specified <see cref="Texture" />, transform <see cref="Matrix2" />, source
		///     <see cref="Rectangle" />, and an optional
		///     <see cref="Color" />, origin <see cref="Vector2" />, <see cref="FlipFlags" />, and depth <see cref="float" />.
		/// </summary>
		/// <param name="texture">The <see cref="Texture" />.</param>
		/// <param name="transformMatrix">The transform <see cref="Matrix2" />.</param>
		/// <param name="sourceRectangle">
		///     The texture region <see cref="Rectangle" /> of the <paramref name="texture" />. Use
		///     <code>null</code> to use the entire <see cref="Texture2D" />.
		/// </param>
		/// <param name="color">The <see cref="Color" />. Use <code>null</code> to use the default <see cref="Color.White" />.</param>
		/// <param name="flags">The <see cref="FlipFlags" />. The default value is <see cref="FlipFlags.None" />.</param>
		/// <param name="depth">The depth <see cref="float" />. The default value is <code>0</code>.</param>
		/// <exception cref="InvalidOperationException">The <see cref="Batcher{TDrawCallInfo}.Begin(ref Matrix, ref Matrix, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect)" /> method has not been called.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="texture" /> is null.</exception>
		public void DrawSprite
		(
			Texture2D texture,
			ref Matrix2 transformMatrix,
			ref Rectangle sourceRectangle,
			Color? color = null,
			FlipFlags flags = FlipFlags.None,
			float depth = 0
		)
		{
			_geometryBuilder.BuildSprite( _vertexCount, ref transformMatrix, texture, ref sourceRectangle, color, flags, depth );
			EnqueueBuiltGeometry( texture, depth );
		}

		/// <summary>
		///     Draws a <see cref="Texture" /> using the specified transform <see cref="Matrix2" /> and an optional
		///     <see cref="Color" />, origin <see cref="Vector2" />, <see cref="FlipFlags" />, and depth <see cref="float" />.
		/// </summary>
		/// <param name="texture">The <see cref="Texture" />.</param>
		/// <param name="transformMatrix">The transform <see cref="Matrix2" />.</param>
		/// <param name="color">The <see cref="Color" />. Use <code>null</code> to use the default <see cref="Color.White" />.</param>
		/// <param name="flags">The <see cref="FlipFlags" />. The default value is <see cref="FlipFlags.None" />.</param>
		/// <param name="depth">The depth <see cref="float" />. The default value is <code>0</code>.</param>
		/// <exception cref="InvalidOperationException">The <see cref="Batcher{TDrawCallInfo}.Begin(ref Matrix, ref Matrix, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect)" /> method has not been called.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="texture" /> is null.</exception>
		public void DrawTexture
		(
			Texture2D texture,
			ref Matrix2 transformMatrix,
			Color? color = null,
			FlipFlags flags = FlipFlags.None,
			float depth = 0
		)
		{
			var rectangle = default( Rectangle );
			_geometryBuilder.BuildSprite( _vertexCount, ref transformMatrix, texture, ref rectangle, color, flags, depth );
			EnqueueBuiltGeometry( texture, depth );
		}
	}
}
