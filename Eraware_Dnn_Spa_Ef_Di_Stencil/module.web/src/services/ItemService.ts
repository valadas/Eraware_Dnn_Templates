import { DnnServicesFramework } from '@eraware/dnn-elements'

export interface IItem {
  Id: number,
  Name: string,
  Description: string
}

export interface IGetItemsResponse {
  items: IItem[],
  page: number,
  resultCount: number,
  pageCount: number,
  CanEdit: boolean
}

export class ItemsService {

  private sf: DnnServicesFramework;
  private requestUrl: string;
  private lastAbortController: any = {};

  /**
   *  Initializes the ItemsService
   */
  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot("$ext_packagename$") + "Item/";
  }

  GetItems = (
    query: string,
    page: number = 1,
    pageSize: number = 10,
    descending: boolean = false
  ) => {
    // First cancel any pending requests to prevent a race condition that could cause out of order responses.
    if (this.lastAbortController.abort) {
      this.lastAbortController.abort();
    }

    // Create an abort controller so we can cancel this request if a new one comes in.
    const currentAbortController = new AbortController();
    this.lastAbortController = currentAbortController;

    return new Promise<IGetItemsResponse>((resolve, reject) => {
      const url = this.requestUrl + "GetItemsPage";
      const queryString = `?query=${query}&page=${page}&pageSize=${pageSize}&descending=${descending}`;

      fetch(url + queryString, {
        headers: this.sf.getModuleHeaders(),
        signal: currentAbortController.signal // Will abort this request if a new request comes in
      })
        .then(response => response.json())
        .then(data => {
          resolve(data);
        })
        .catch(error => reject(error));
    })
  }

  CreateItem = (
    item: IItem
  ) => {
    return new Promise((resolve, reject) => {
      const url = this.requestUrl + "CreateItem";
      let headers = this.sf.getModuleHeaders();
      headers.append('Content-Type', 'application/json');

      fetch(url, {
        method: "POST",
        body: JSON.stringify(item),
        headers: headers
      })
        .then(() => resolve())
        .catch(error => reject(error));
    })
  }

  DeleteItem = (item: IItem) => {
    return new Promise((resolve, reject) => {
      const url = this.requestUrl + "DeleteItem";
      let headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");

      fetch(url, {
        method: 'POST',
        body: JSON.stringify(item),
        headers: headers
      })
        .then(() => resolve())
        .catch(error => reject(error))
    })
  }
}
