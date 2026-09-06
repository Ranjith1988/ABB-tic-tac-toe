import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MoveHistoryComponent } from './move-history.component';

describe('MoveHistoryComponent', () => {
  let fixture: ComponentFixture<MoveHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [MoveHistoryComponent] }).compileComponents();
    fixture = TestBed.createComponent(MoveHistoryComponent);
  });

  it('shows an empty state', () => {
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No moves yet.');
  });

  it('renders move numbers, marks and positions', () => {
    fixture.componentInstance.moves = [
      { moveNumber: 1, player: 'X', row: 0, column: 2 },
      { moveNumber: 2, player: 'O', row: 1, column: 1 }
    ];
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('#1');
    expect(fixture.nativeElement.textContent).toContain('Row 1, Column 3');
    expect(fixture.nativeElement.querySelectorAll('.player-mark').length).toBe(2);
  });
});