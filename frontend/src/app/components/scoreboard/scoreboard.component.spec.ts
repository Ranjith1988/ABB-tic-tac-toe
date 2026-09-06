import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScoreboardComponent } from './scoreboard.component';

describe('ScoreboardComponent', () => {
  let fixture: ComponentFixture<ScoreboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ScoreboardComponent] }).compileComponents();
    fixture = TestBed.createComponent(ScoreboardComponent);
    fixture.componentInstance.scoreboard = { xWins: 2, oWins: 1, draws: 3 };
    fixture.detectChanges();
  });

  it('renders all score categories', () => {
    expect(fixture.nativeElement.textContent).toContain('2');
    expect(fixture.nativeElement.textContent).toContain('1');
    expect(fixture.nativeElement.textContent).toContain('3');
    expect(fixture.nativeElement.textContent).toContain('X Wins');
    expect(fixture.nativeElement.textContent).toContain('O Wins');
    expect(fixture.nativeElement.textContent).toContain('Draws');
  });
});