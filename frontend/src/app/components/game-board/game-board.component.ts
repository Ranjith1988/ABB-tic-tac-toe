import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Player } from '../../models/game.models';

@Component({
  selector: 'ttt-game-board',
  standalone: true,
  templateUrl: './game-board.component.html',
  styleUrl: './game-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameBoardComponent {
  @Input({ required: true }) board!: Array<Player | null>;
  @Input() winningCells: number[] = [];
  @Input() disabled = false;
  @Output() cellSelected = new EventEmitter<number>();

  isWinningCell(index: number): boolean {
    return this.winningCells.includes(index);
  }
}
