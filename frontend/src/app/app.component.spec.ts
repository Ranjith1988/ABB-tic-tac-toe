import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AppComponent } from './app.component';
import { GameApiService } from './services/game-api.service';
import { of } from 'rxjs';

class GameApiServiceStub {
  createGame() {
    return of({
      gameId: 'game-1', board: Array(9).fill(null), currentPlayer: 'X', mode: 'TwoPlayer',
      status: 'InProgress', winner: null, winningCells: [], moveHistory: [],
      scoreboard: { xWins: 0, oWins: 0, draws: 0 }
    });
  }
  makeMove() { return this.createGame(); }
  undo() { return this.createGame(); }
  resetGame() { return this.createGame(); }
  resetScoreboard() { return of(void 0); }
  getScoreboard() { return of({ xWins: 0, oWins: 0, draws: 0 }); }
}

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: GameApiService, useClass: GameApiServiceStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
  });

  it('renders the board and initial turn', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelectorAll('.cell').length).toBe(9);
    expect(element.querySelector('.status')?.textContent).toContain("Player X's turn");
  });
});
