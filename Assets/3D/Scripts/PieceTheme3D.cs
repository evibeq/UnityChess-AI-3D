using UnityEngine;

namespace Chess.Game {
    [CreateAssetMenu(menuName = "Theme/Pieces3D")]
    public class PieceTheme3D : ScriptableObject {

		public PiecePrefabs whitePieces;
		public PiecePrefabs blackPieces;

		public GameObject GetPiecePrefab(int piece)
		{
			PiecePrefabs piecePrefabs = Piece.IsColour(piece, Piece.White) ? whitePieces : blackPieces;

			switch (Piece.PieceType(piece))
			{
				case Piece.Pawn:
					return piecePrefabs.pawn;
				case Piece.Rook:
					return piecePrefabs.rook;
				case Piece.Knight:
					return piecePrefabs.knight;
				case Piece.Bishop:
					return piecePrefabs.bishop;
				case Piece.Queen:
					return piecePrefabs.queen;
				case Piece.King:
					return piecePrefabs.king;
				default:
					if (piece != 0)
					{
						Debug.Log(piece);
					}
					return null;
			}
		}

		[System.Serializable]
        public class PiecePrefabs
        {
            public GameObject pawn, rook, knight, bishop, queen, king;

            public GameObject this[int i]
            {
                get
                {
                    return new GameObject[] { pawn, rook, knight, bishop, queen, king }[i];
                }
            }
        }
    }
}

