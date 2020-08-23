export default function ({ $auth, $axios }) {
  configureAuth($auth);
  register401ResponseInterceptor($axios, $auth);

}

function configureAuth($auth) {
  if (window.config) {
    $auth.strategies.oAuth2.options.authorization_endpoint = window.config.authorization_endpoint;
    $auth.strategies.oAuth2.options.client_id = window.config.authorization_client_id;
  }
}

function register401ResponseInterceptor($axios, $auth) {
  $axios.interceptors.response.use(
    response => response,
    error => {
      if (error.response.status === 401) {
        $auth.logout();
        window.location = window.location.origin + '/login';
      }
    }
  );  
}