using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
    public class Board3D : MonoBehaviour
    {
        public PieceTheme3D pieceTheme;
        public BoardTheme3D boardTheme;
        public bool showLegalMoves;

        public bool whiteIsBottom = true;

        public GameObject[,] squareObjects;
        public GameObject[,] squarePieceObjects;
        Move lastMadeMove;
        MoveGenerator moveGenerator;

        public float pieceDepth = 1f;
        public float pieceDragDepth = 0f;

		void Awake()
		{
			moveGenerator = new MoveGenerator();
			CreateBoard();
		}

		public void HighlightLegalMoves(Board board, Coord fromSquare)
		{
			if (showLegalMoves)
			{

				var moves = moveGenerator.GenerateMoves(board);

				for (int i = 0; i < moves.Count; i++)
				{
					Move move = moves[i];
					if (move.StartSquare == BoardRepresentation.IndexFromCoord(fromSquare))
					{
						Coord coord = BoardRepresentation.CoordFromIndex(move.TargetSquare);
						SetSquareColour(coord, boardTheme.lightSquares.legal, boardTheme.darkSquares.legal);
					}
				}
			}
		}

		public void DragPiece(Coord pieceCoord, Vector3 mousePos)
		{
			squarePieceObjects[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = new Vector3(mousePos.x, pieceDragDepth, mousePos.z);
		}

		public void ResetPiecePosition(Coord pieceCoord)
		{
			Vector3 pos = PositionFromCoord(pieceCoord.fileIndex, pieceDepth, pieceCoord.rankIndex);
			squarePieceObjects[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = pos;
		}

		public void SelectSquare(Coord coord)
		{
			SetSquareColour(coord, boardTheme.lightSquares.selected, boardTheme.darkSquares.selected);
		}

		public void DeselectSquare(Coord coord)
		{
			//BoardTheme.SquareColours colours = (coord.IsLightSquare ()) ? boardTheme.lightSquares : boardTheme.darkSquares;
			//squareMaterials[coord.file, coord.rank].color = colours.normal;
			ResetSquareColours();
		}

		public bool TryGetSquareUnderMouse(Vector3 squarePos, out Coord selectedCoord)
		{
			int file = (int)(squarePos.x);
			int rank = (int)(squarePos.z);
			if (!whiteIsBottom)
			{
				file = 7 - file;
				rank = 7 - rank;
			}
			selectedCoord = new Coord(file, rank);
			return file >= 0 && file < 8 && rank >= 0 && rank < 8;
		}

		public void UpdatePosition(Board board)
		{
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					Coord coord = new Coord(file, rank);
					int piece = board.Square[BoardRepresentation.IndexFromCoord(coord.fileIndex, coord.rankIndex)];

					int childs = squarePieceObjects[file, rank].transform.childCount;
					for (int i = 0; i < childs; i++)
					{
                        Destroy(squarePieceObjects[file, rank].transform.GetChild(i).gameObject);
					}

					if (pieceTheme.GetPiecePrefab(piece) != null)
                    {
						GameObject newPiece = Instantiate(pieceTheme.GetPiecePrefab(piece));
						newPiece.transform.parent = squarePieceObjects[file, rank].transform;
						newPiece.transform.localPosition = Vector3.zero;
					}
				}
			}

		}

		public void OnMoveMade(Board board, Move move, bool animate = false)
		{
			lastMadeMove = move;
			if (animate)
			{
				StartCoroutine(AnimateMove(move, board));
			}
			else
			{
				UpdatePosition(board);
				ResetSquareColours();
			}
		}

		IEnumerator AnimateMove(Move move, Board board)
		{
			float t = 0;
			const float moveAnimDuration = 0.15f;
			Coord startCoord = BoardRepresentation.CoordFromIndex(move.StartSquare);
			Coord targetCoord = BoardRepresentation.CoordFromIndex(move.TargetSquare);
			Transform pieceT = squarePieceObjects[startCoord.fileIndex, startCoord.rankIndex].transform;
			Vector3 startPos = PositionFromCoord(startCoord, pieceDepth);
			Vector3 targetPos = PositionFromCoord(targetCoord, pieceDepth);
			SetSquareColour(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.lightSquares.moveFromHighlight, boardTheme.darkSquares.moveFromHighlight);

			while (t <= 1)
			{
				yield return null;
				t += Time.deltaTime * 1 / moveAnimDuration;
				pieceT.position = Vector3.Lerp(startPos, targetPos, t);
			}
			UpdatePosition(board);
			ResetSquareColours();
			pieceT.position = startPos;
		}

		void HighlightMove(Move move)
		{
			SetSquareColour(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.lightSquares.moveFromHighlight, boardTheme.darkSquares.moveFromHighlight);
			SetSquareColour(BoardRepresentation.CoordFromIndex(move.TargetSquare), boardTheme.lightSquares.moveToHighlight, boardTheme.darkSquares.moveToHighlight);
		}

		void CreateBoard()
		{

			Shader squareShader = Shader.Find("Unlit/Color");
			squareObjects = new GameObject[8, 8];
			squarePieceObjects = new GameObject[8, 8];

			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					// Create square
					Transform square = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
					square.parent = transform;
					square.name = BoardRepresentation.SquareNameFromCoordinate(file, rank);
					square.position = PositionFromCoord(file, 0, rank);
					Material squareMaterial = new Material(squareShader);

					squareObjects[file, rank] = square.gameObject;
					squareObjects[file, rank].GetComponent<Renderer>().material = squareMaterial;

					// Create piece sprite renderer for current square
					GameObject pieceObject = new GameObject("Piece");
					pieceObject.transform.parent = square;
					pieceObject.transform.position = PositionFromCoord(file, pieceDepth, rank);
					squarePieceObjects[file, rank] = pieceObject;
				}
			}

			ResetSquareColours();
		}

		void ResetSquarePositions()
		{
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					if (file == 0 && rank == 0)
					{
						//Debug.Log (squarePieceRenderers[file, rank].gameObject.name + "  " + PositionFromCoord (file, rank, pieceDepth));
					}
					//squarePieceRenderers[file, rank].transform.position = PositionFromCoord (file, rank, pieceDepth);
					squareObjects[file, rank].transform.position = PositionFromCoord(file, 0, rank);
					squarePieceObjects[file, rank].transform.position = PositionFromCoord(file, pieceDepth, rank);
				}
			}

			if (!lastMadeMove.IsInvalid)
			{
				HighlightMove(lastMadeMove);
			}
		}

		public void SetPerspective(bool whitePOV)
		{
			whiteIsBottom = whitePOV;
			ResetSquarePositions();

		}

		public void ResetSquareColours(bool highlight = true)
		{
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					SetSquareColour(new Coord(file, rank), boardTheme.lightSquares.normal, boardTheme.darkSquares.normal);
				}
			}
			if (highlight)
			{
				if (!lastMadeMove.IsInvalid)
				{
					HighlightMove(lastMadeMove);
				}
			}
		}

		void SetSquareColour(Coord square, Color lightCol, Color darkCol)
		{
			squareObjects[square.fileIndex, square.rankIndex].GetComponent<Renderer>().material.color = (square.IsLightSquare()) ? lightCol : darkCol;
		}

		public Vector3 PositionFromCoord(int file, float depth, int rank)
		{
			if (whiteIsBottom)
			{
				return new Vector3(file, depth, rank);
			}
			return new Vector3(7 - file, depth, 7 - rank);
		}

		public Vector3 PositionFromCoord(Coord coord, float depth)
		{
			return PositionFromCoord(coord.fileIndex, depth, coord.rankIndex);
		}
	}
}
