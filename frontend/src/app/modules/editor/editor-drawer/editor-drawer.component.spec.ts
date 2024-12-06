import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditorDrawerComponent } from './editor-drawer.component';

describe('EditorDrawerComponent', () => {
  let component: EditorDrawerComponent;
  let fixture: ComponentFixture<EditorDrawerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditorDrawerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditorDrawerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
