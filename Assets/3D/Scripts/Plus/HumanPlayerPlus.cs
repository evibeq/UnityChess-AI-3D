using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
{
	public class HumanPlayerPlus : Player
	{

		public enum InputState
		{
			None,
			PieceSelected,
			DraggingPiece
		}

		InputState currentState;

		BoardPlus board3D;
		Camera cam;
		Coord selectedPieceSquare;
		Board board;
		public HumanPlayerPlus(Board board)
		{
			board3D = Object.FindObjectOfType<BoardPlus>();
			cam = Camera.main;
			this.board = board;
		}

		public override void NotifyTurnToMove()
		{

		}

		public override void Update()
		{
			HandleInput();
		}

		void HandleInput()
		{
			RaycastHit hit;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			Vector3 mousePos, squarePos;

			if (Physics.Raycast(ray, out hit))
			{
				mousePos = hit.point;
				squarePos = hit.transform.position;

				if (currentState == InputState.None)
				{
					HandlePieceSelection(squarePos);
				}
				else if (currentState == InputState.DraggingPiece)
				{
					HandleDragMovement(mousePos, squarePos);
				}
				else if (currentState == InputState.PieceSelected)
				{
					HandlePointAndClickMovement(squarePos);
				}

				if (Input.GetMouseButtonDown(1))
				{
					CancelPieceSelection();
				}

			}
		}

		void HandlePointAndClickMovement(Vector3 mousePos)
		{
			if (Input.GetMouseButton(0))
			{
				HandlePiecePlacement(mousePos);
			}
		}

		void HandleDragMovement(Vector3 mousePos, Vector3 squarePos)
		{
			board3D.DragPiece(selectedPieceSquare, mousePos);
			// If mouse is released, then try place the piece
			if (Input.GetMouseButtonUp(0))
			{
				HandlePiecePlacement(squarePos);
			}
		}

		void HandlePiecePlacement(Vector3 squarePos)
		{
			Coord targetSquare;
			if (board3D.TryGetSquareUnderMouse(squarePos, out targetSquare))
			{
				if (targetSquare.Equals(selectedPieceSquare))
				{
					board3D.ResetPiecePosition(selectedPieceSquare);
					if (currentState == InputState.DraggingPiece)
					{
						currentState = InputState.PieceSelected;
					}
					else
					{
						currentState = InputState.None;
						board3D.DeselectSquare(selectedPieceSquare);
					}
				}
				else
				{
					int targetIndex = BoardRepresentation.IndexFromCoord(targetSquare.fileIndex, targetSquare.rankIndex);
					if (Piece.IsColour(board.Square[targetIndex], board.ColourToMove) && board.Square[targetIndex] != 0)
					{
						CancelPieceSelection();
						HandlePieceSelection(squarePos);
					}
					else
					{
						board3D.ResetPiecePosition(selectedPieceSquare);
						TryMakeMove(selectedPieceSquare, targetSquare);
					}
				}
			}
			else
			{
				CancelPieceSelection();
			}

		}

		void CancelPieceSelection()
		{
			if (currentState != InputState.None)
			{
				currentState = InputState.None;
				board3D.DeselectSquare(selectedPieceSquare);
				board3D.ResetPiecePosition(selectedPieceSquare);
			}
		}

		void TryMakeMove(Coord startSquare, Coord targetSquare)
		{
			int startIndex = BoardRepresentation.IndexFromCoord(startSquare);
			int targetIndex = BoardRepresentation.IndexFromCoord(targetSquare);
			bool moveIsLegal = false;
			Move chosenMove = new Move();

			MoveGenerator moveGenerator = new MoveGenerator();
			bool wantsKnightPromotion = Input.GetKey(KeyCode.LeftAlt);

			var legalMoves = moveGenerator.GenerateMoves(board);
			for (int i = 0; i < legalMoves.Count; i++)
			{
				var legalMove = legalMoves[i];

				if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex)
				{
					if (legalMove.IsPromotion)
					{
						if (legalMove.MoveFlag == Move.Flag.PromoteToQueen && wantsKnightPromotion)
						{
							continue;
						}
						if (legalMove.MoveFlag != Move.Flag.PromoteToQueen && !wantsKnightPromotion)
						{
							continue;
						}
					}
					moveIsLegal = true;
					chosenMove = legalMove;
					//	Debug.Log (legalMove.PromotionPieceType);
					break;
				}
			}

			if (moveIsLegal)
			{
				ChoseMove(chosenMove);
				currentState = InputState.None;
			}
			else
			{
				CancelPieceSelection();
			}
		}

		void HandlePieceSelection(Vector3 mousePos)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (board3D.TryGetSquareUnderMouse(mousePos, out selectedPieceSquare))
				{
					int index = BoardRepresentation.IndexFromCoord(selectedPieceSquare);
					// If square contains a piece, select that piece for dragging
					if (Piece.IsColour(board.Square[index], board.ColourToMove))
					{
						board3D.HighlightLegalMoves(board, selectedPieceSquare);
						board3D.SelectSquare(selectedPieceSquare);
						currentState = InputState.DraggingPiece;
					}
				}
			}
		}
	}
}
