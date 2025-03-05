type IAuthInterceptor = {
  getToken?: () => Promise<any>;
  intercept: (config: any) => Promise<any>;
};

export const AuthInterceptor: IAuthInterceptor = {
  getToken: undefined,

  intercept: async (config) => {
    if (!AuthInterceptor.getToken) {
      return config;
    }

    try {
      const token = await AuthInterceptor.getToken();
      config.headers["Authorization"] = `Bearer ${token}`;
    } catch (e) {
      console.log(e);
    }
    return config;
  },
};
