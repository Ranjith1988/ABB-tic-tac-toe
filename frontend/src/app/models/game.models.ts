export type Player = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'Computer';
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface Move {
  player: Player;
  row: number;
  column: number;
  moveNumber: number;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  gameId: string;
  board: Array<Player | null>;
  currentPlayer: Player;
  mode: GameMode;
  status: GameStatus;
  winner: Player | null;
  winningCells: number[];
  moveHistory: Move[];
  scoreboard: Scoreboard;
}

export interface ApiError {
  code: string;
  message: string;
}
