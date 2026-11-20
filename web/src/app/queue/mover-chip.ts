import { Component, computed, input } from '@angular/core';
import { Mover } from '../api/models';
import { moverLabel, signedPoints } from './queue-logic';

@Component({
  selector: 'app-mover-chip',
  template: `<span class="chip" [attr.aria-label]="label()"><span [class]="up() ? 'up' : 'down'" aria-hidden="true">{{ up() ? '▲' : '▼' }} {{ points() }}</span> {{ mover().criterion }} · {{ mover().evidence }}</span>`,
})
export class MoverChip {
  readonly mover = input.required<Mover>();
  protected readonly up = computed(() => this.mover().points >= 0);
  protected readonly points = computed(() => signedPoints(this.mover().points));
  protected readonly label = computed(() => moverLabel(this.mover()));
}
