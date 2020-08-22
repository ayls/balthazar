export default {
  mode: 'spa',
  env: {
    apiBase: 'http://localhost:7071/api'
  },
  /*
   ** Headers of the page
   */
  head: {
    title: process.env.npm_package_name || '',
    meta: [
      { charset: 'utf-8' },
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      {
        hid: 'description',
        name: 'description',
        content: process.env.npm_package_description || ''
      }
    ],
    link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }],
    script: [
      { src: '/config.js' }      
    ]
  },
  /*
   ** Customize the progress-bar color
   */
  loading: { color: '#fff' },
  /*
   ** Global CSS
   */
  css: ['element-ui/lib/theme-chalk/index.css'],
  /*
   ** Nuxt.js dev-modules
   */
  buildModules: ['@nuxt/typescript-build'],
  /*
   ** Nuxt.js modules
   */
  modules: [
    // Doc: https://axios.nuxtjs.org/usage
    '@nuxtjs/axios',
    // Doc: https://axios.nuxtjs.org/usage 
    '@nuxtjs/auth'    
  ],
  /*
   ** Axios module configuration
   ** See https://axios.nuxtjs.org/options
   */
  axios: {},
  /*
   ** Axios module configuration
   ** See https://auth.nuxtjs.org/
   */
  auth: {
    plugins: [ '~/plugins/auth.js' ],
    strategies: {
      local: false,
      oAuth2: {
        _scheme: 'oauth2',
        authorization_endpoint: 'http://localhost:5001/auth',
        userinfo_endpoint: false,
        scope: ['openid'],
        access_type: undefined,
        access_token_endpoint: undefined,
        response_type: 'id_token',
        token_type: 'Bearer',
        redirect_uri: undefined,
        client_id: 'balthazar-dev',
        token_key: 'id_token',
        state: 'UNIQUE_AND_NON_GUESSABLE'
      }      
    }
  },
  /*
   ** Router config
   */  
  router: {
    middleware: ['auth']
  },
  /*
   ** Plugins to load before mounting the App
   */
  plugins: [
    '@/plugins/element-ui',
    '@/plugins/axios.js', // configures Axios
  ],    
  /*
   ** Build configuration
   */
  build: {
    transpile: [/^element-ui/],
    /*
     ** You can extend webpack config here
     */
    extend(config, ctx) {
      if (ctx.isDev) {
        config.devtool = ctx.isClient ? 'source-map' : 'inline-source-map'
      }      
    }
  }
}
