import { TestBed } from '@angular/core/testing';

import { CertificateObserverService } from './certificate-observer.service';

describe('CertificateObserverService', () => {
  let service: CertificateObserverService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CertificateObserverService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
