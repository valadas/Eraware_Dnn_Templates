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

    dnnHeaders.forEach((value, key) => {
      options.headers[key] = value;
    });

    return Promise.resolve(options);
  }
}

export interface ConfigureRequest {
  moduleId: number;
} 
