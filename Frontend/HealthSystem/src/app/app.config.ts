import { ApplicationConfig, provideZoneChangeDetection, ErrorHandler } from '@angular/core';
import { provideRouter } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, withFetch } from '@angular/common/http';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';


//Bugsnag
import { BugsnagErrorHandler } from '@bugsnag/plugin-angular';
export function errorHandlerFactory() {
  return new BugsnagErrorHandler();
}

export const appConfig: ApplicationConfig = {
  providers: [{ provide: ErrorHandler, useFactory: errorHandlerFactory }, ReactiveFormsModule, provideHttpClient(withFetch()), provideZoneChangeDetection({ eventCoalescing: true }), provideRouter(routes), provideClientHydration(withEventReplay())
    , provideRouter(routes), provideClientHydration(withEventReplay()), provideHttpClient()]
};



