import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';

//Bugsnag
import Bugsnag from '@bugsnag/js';
Bugsnag.start({ apiKey: 'a3e817ac8adf630ad339055527ef6ca1' });
console.log('✅ Bugsnag has been started');

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient()
  ]
});

/*bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));*/
