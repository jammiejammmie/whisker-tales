// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

namespace GameVanilla.Game.UI
{
	/// <summary>
	/// This class manages the girl displayed during a game at the top bar of the UI.
	/// </summary>
	public class Girl : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField]
		private ParticleSystem particles;
#pragma warning restore 649

		/// <summary>
		/// Unity's Awake method.
		/// </summary>
		private void Awake()
		{
			Assert.IsNotNull(particles);
		}

		/// <summary>
		/// Plays the "happy" particle system.
		/// </summary>
		public void PlayParticles()
		{
			particles.Play();
		}
	}
}
