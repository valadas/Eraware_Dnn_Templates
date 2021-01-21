import { DnnServicesFramework } from '@eraware/dnn-elements';
export class ClientBase {

  private sf: DnnServicesFramework;
  private moduleId: number;

  constructor(configuration: ConfigureRequest) {
    this.moduleId = configuration.moduleId;
    this.sf = new DnnServicesFramework(this.moduleId);
  }

  protected getBaseUrl(_defaultUrl: string, baseUrl?: string) {
    return baseUrl || "";
  }

  protected transformOptions(options: RequestInit): Promise<RequestInit> {
    var dnnHeaders = this.sf.getModuleHeaders();

    dnnHeaders.forEach((value, key) => {
      options.headers[key] = value;
    });

    return Promise.resolve(options);
  }
}

export interface ConfigureRequest {
  moduleId: number;
} 
