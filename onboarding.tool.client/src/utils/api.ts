export class Api {
  private readonly baseUrl: URL;

  constructor(base: string) {
    if (base.match(/^https?:\/\//)) {
      this.baseUrl = new URL(base);
    } else {
      if (base.startsWith("/")) {
        this.baseUrl = new URL(base, window.location.origin);
      } else {
        this.baseUrl = new URL(
          base,
          window.location.origin + window.location.pathname
        );
      }
    }

    if (!this.baseUrl.pathname.endsWith("/")) {
      this.baseUrl.pathname += "/";
    }
  }

  get<T>(
    url: string,
    data?: Record<string, string>,
    options?: RequestInit
  ): Promise<T> {
    if (data) {
      const searchParams = new URLSearchParams(data);
      url += `?${searchParams}`;
    }

    return this.request(url, {
      method: "GET",
      ...options,
    });
  }

  post<T, TData>(url: string, data: TData, options?: RequestInit): Promise<T> {
    return this.request(url, {
      method: "POST",
      body: JSON.stringify(data),
      ...options,
    });
  }

  private async request<T>(url: string, options: RequestInit = {}): Promise<T> {
    const headers = {
      "Content-Type": "application/json",
      Accept: "application/json",
      "X-Requested-With": "XMLHttpRequest",
      ...(options.headers || {}),
    };

    if (url.startsWith("/")) {
      url = url.slice(1);
    }

    const fullUrl = new URL(url, this.baseUrl);

    const makeFetch = async (): Promise<Response> => {
      return fetch(fullUrl.toString(), {
        ...options,
        headers,
      });
    };

    let response = await makeFetch();

    if (!response.ok) {
      throw new Error(response.statusText);
    }

    return response.json();
  }
}
