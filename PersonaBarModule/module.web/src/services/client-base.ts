import { DnnServicesFramework } from '@dnncommunity/dnn-elements';

export class ClientBase {

  private sf: DnnServicesFramework;
  private moduleId: number;

  constructor(configuration: ConfigureRequest) {
    this.moduleId = configuration.moduleId;
    this.sf = new DnnServicesFramework(this.moduleId);
  }

  protected getBaseUrl(_defaultUrl: string, baseUrl?: string): string {
    baseUrl = this.sf.getServiceRoot("Eraware_MyPersonaBarModule");

    // Strips the last / if present for future concatenations
    baseUrl = baseUrl.replace(/\/$/, "");

    return baseUrl || "";
  }

  protected transformOptions(options: RequestInit): Promise<RequestInit> {
    const dnnHeaders = this.sf.getModuleHeaders();

    // Handle different types of headers
    if (!options.headers) {
      // Create a plain object since most fetch consumers expect this
      options.headers = {};
    }

    // Check if headers is a Headers instance
    if (options.headers instanceof Headers) {
      // Work with Headers instance using its methods
      dnnHeaders.forEach((value: string, key: string) => {
        (options.headers as Headers).set(key, value);
      });
    } else {
      // Handle as plain object
      const headersObj = options.headers as Record<string, string>;
      dnnHeaders.forEach((value: string, key: string) => {
        headersObj[key] = value;
      });
    }

    return Promise.resolve(options);
  }
}

export interface ConfigureRequest {
  moduleId: number;
} 
