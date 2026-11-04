import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';

type ApiStatus = 'loading' | 'ok' | 'error';

@Component({
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly http = inject(HttpClient);

  protected readonly status = signal<ApiStatus>('loading');
  protected readonly version = signal('');

  ngOnInit(): void {
    this.http.get<{ version: string }>('/api/version').subscribe({
      next: (info) => {
        this.version.set(info.version);
        this.status.set('ok');
      },
      error: () => this.status.set('error'),
    });
  }
}
