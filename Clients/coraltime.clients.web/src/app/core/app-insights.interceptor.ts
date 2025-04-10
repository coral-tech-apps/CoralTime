import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { AppInsightsService } from '../services/app-insights.service';
import { Observable} from 'rxjs';

@Injectable()
export class AppInsightsInterceptor implements HttpInterceptor {
    constructor(private appInsightsService: AppInsightsService) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
      try{
        if(this.appInsightsService){
          this.appInsightsService.trackEvent(
            req.url,
            { body: req.body }
          );
        }else{
          console.warn("not initialized")
        }
      } catch(e){
        console.error(e);
      }
        return next.handle(req);
    }
}
