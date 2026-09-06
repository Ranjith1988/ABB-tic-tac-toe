import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { ApiError, GameMode, GameState, Player, Scoreboard } from '../models/game.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GameApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  createGame(mode: GameMode): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games`, { mode }).pipe(catchError(this.handleError));
  }

  getGame(gameId: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/games/${gameId}`).pipe(catchError(this.handleError));
  }

  makeMove(gameId: string, player: Player, row: number, column: number): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/moves`, { player, row, column }).pipe(catchError(this.handleError));
  }

  undo(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/undo`, {}).pipe(catchError(this.handleError));
  }

  resetGame(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/reset`, {}).pipe(catchError(this.handleError));
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.baseUrl}/scoreboard`).pipe(catchError(this.handleError));
  }

  resetScoreboard(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/scoreboard/reset`, {}).pipe(catchError(this.handleError));
  }

  private handleError = (error: HttpErrorResponse) => {
    const apiError = error.error as ApiError | null;
    const message = apiError?.message ?? 'Unable to complete the request. Please check that the API is running.';
    return throwError(() => new Error(message));
  };
}
