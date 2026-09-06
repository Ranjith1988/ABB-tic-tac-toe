import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Move } from '../../models/game.models';

@Component({
  selector: 'ttt-move-history',
  standalone: true,
  templateUrl: './move-history.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MoveHistoryComponent {
  @Input() moves: Move[] = [];
}
