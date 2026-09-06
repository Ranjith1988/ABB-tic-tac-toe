import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Scoreboard } from '../../models/game.models';

@Component({
  selector: 'ttt-scoreboard',
  standalone: true,
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ScoreboardComponent {
  @Input({ required: true }) scoreboard!: Scoreboard;
}
