/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;

namespace Poly2Tri
{
	public abstract class TriangulationContext
	{
		private TriangulationDebugContext local_DebugContext = null;
		public TriangulationDebugContext DebugContext 
		{
			get { return local_DebugContext; }
			protected set { local_DebugContext = value; }
		}

		public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();
		public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);
		private TriangulationMode local_TriangulationMode;
		public TriangulationMode TriangulationMode 
		{
			get { return local_TriangulationMode; }
			protected set { local_TriangulationMode = value; }
		}
		private Triangulatable local_Triangulatable = null;
		public Triangulatable Triangulatable 
		{
			get { return local_Triangulatable; }
			private set { local_Triangulatable = value; }
		}

		private int local_Step;
		public int StepCount 
		{
			get { return local_Step; }
			private set { local_Step = value; }
		}

		public void Done()
		{
			StepCount++;
		}

		public abstract TriangulationAlgorithm Algorithm { get; }

		public virtual void PrepareTriangulation(Triangulatable t)
		{
			Triangulatable = t;
			TriangulationMode = t.TriangulationMode;
			t.Prepare(this);
		}

		public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);

		public void Update(string message) { }

		public virtual void Clear()
		{
			Points.Clear();
			if (DebugContext != null) DebugContext.Clear();
			StepCount = 0;
		}

		private bool local_IsDebugEnabled;
		public virtual bool IsDebugEnabled
		{
			get { return local_IsDebugEnabled; }
			protected set { local_IsDebugEnabled = value; }
		}

		public DTSweepDebugContext DTDebugContext { get { return DebugContext as DTSweepDebugContext; } }
	}
}