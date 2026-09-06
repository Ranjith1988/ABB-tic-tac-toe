import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameBoardComponent } from './game-board.component';

describe('GameBoardComponent', () => {
  let fixture: ComponentFixture<GameBoardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [GameBoardComponent] }).compileComponents();
    fixture = TestBed.createComponent(GameBoardComponent);
    fixture.componentInstance.board = ['X', null, 'O', null, null, null, null, null, null];
    fixture.componentInstance.winningCells = [0, 2];
    fixture.detectChanges();
  });

  it('renders marks and winning cells', () => {
    const cells = fixture.nativeElement.querySelectorAll('.cell') as NodeListOf<HTMLButtonElement>;
    expect(cells.length).toBe(9);
    expect(cells[0].textContent?.trim()).toBe('X');
    expect(cells[0].classList.contains('winner')).toBeTrue();
    expect(cells[1].disabled).toBeFalse();
    expect(cells[2].disabled).toBeTrue();
  });

  it('emits the selected cell', () => {
    const selected: number[] = [];
    fixture.componentInstance.cellSelected.subscribe(index => selected.push(index));

    (fixture.nativeElement.querySelectorAll('.cell')[1] as HTMLButtonElement).click();

    expect(selected).toEqual([1]);
  });

  it('disables all cells when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const cells = Array.from(fixture.nativeElement.querySelectorAll('.cell')) as HTMLButtonElement[];
    expect(cells.every(cell => cell.disabled)).toBeTrue();
  });
});