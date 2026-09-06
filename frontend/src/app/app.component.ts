import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameBoardComponent } from './components/game-board/game-board.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { GameApiService } from './services/game-api.service';
import { GameMode, GameState, Player } from './models/game.models';

@Component({
  selector: 'ttt-root',
  standalone: true,
  imports: [FormsModule, GameBoardComponent, MoveHistoryComponent, ScoreboardComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit {
  private readonly api = inject(GameApiService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  game: GameState | null = null;
  selectedMode: GameMode = 'TwoPlayer';
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.startGame();
  }

  get canUndo(): boolean {
    return !!this.game && this.game.moveHistory.length > 0 && this.game.status === 'InProgress';
  }

  get gameMessage(): string {
    if (!this.game) return 'Starting game…';
    if (this.game.status === 'Won') return `Player ${this.game.winner} wins!`;
    if (this.game.status === 'Draw') return 'It is a draw!';
    return this.game.mode === 'Computer' && this.game.currentPlayer === 'O'
      ? 'Computer is thinking…'
      : `Player ${this.game.currentPlayer}'s turn`;
  }

  startGame(): void {
    this.run(() => this.api.createGame(this.selectedMode));
  }

  selectCell(index: number): void {
    if (!this.game || this.loading || this.game.status !== 'InProgress') return;
    if (this.game.board[index] !== null) return;
    if (this.game.mode === 'Computer' && this.game.currentPlayer !== 'X') return;

    const row = Math.floor(index / 3);
    const column = index % 3;
    this.run(() => this.api.makeMove(this.game!.gameId, this.game!.currentPlayer, row, column));
  }

  undo(): void {
    if (!this.game || !this.canUndo || this.loading) return;
    this.run(() => this.api.undo(this.game!.gameId));
  }

  resetGame(): void {
    if (!this.game || this.loading) return;
    this.run(() => this.api.resetGame(this.game!.gameId));
  }

  resetScoreboard(): void {
    if (this.loading) return;
    this.loading = true;
    this.errorMessage = '';
    this.api.resetScoreboard().subscribe({
      next: () => {
        if (this.game) {
          this.api.getGame(this.game.gameId).subscribe({
            next: game => { this.game = game; this.loading = false; this.changeDetector.markForCheck(); },
            error: error => { this.errorMessage = error.message; this.loading = false; this.changeDetector.markForCheck(); }
          });
        } else {
          this.loading = false;
          this.changeDetector.markForCheck();
        }
      },
      error: error => { this.errorMessage = error.message; this.loading = false; this.changeDetector.markForCheck(); }
    });
  }

  cellLabel(index: number): string {
    const row = Math.floor(index / 3) + 1;
    const column = (index % 3) + 1;
    const value = this.game?.board[index];
    return value ? `Row ${row}, Column ${column}, ${value}` : `Row ${row}, Column ${column}, empty`;
  }

  private run(request: () => ReturnType<GameApiService['createGame']>): void {
    this.loading = true;
    this.errorMessage = '';
    request().subscribe({
      next: game => { this.game = game; this.loading = false; this.changeDetector.markForCheck(); },
      error: error => { this.errorMessage = error.message; this.loading = false; this.changeDetector.markForCheck(); }
    });
  }
}
