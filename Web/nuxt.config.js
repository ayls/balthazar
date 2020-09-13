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
    link: [{ rel: 'icon', type: 'image/png', href: '/favicon.png' }],
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
    // Doc: https://axios.nuxtjs.org
    '@nuxtjs/axios',
    // Doc: https://dev.auth.nuxtjs.org
    '@nuxtjs/auth-next'    
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
    redirect: {
      login: '/login',
      logout: '/login',
      callback: '/callback',
      home: '/'
    },    
    strategies: {
      local: false,
      oAuth2: {
        scheme: 'oauth2',
        endpoints: {
          authorization: 'http://localhost:5001/auth',
          token: false,
          userinfo: false,
          logout: false
        },
        token: {
          property: 'id_token',
          type: 'Bearer'   
        },
        responseType: 'id_token',        
        clientId: 'balthazar-dev',
        scope: ['openid'],
        state: 'UNIQUE_AND_NON_GUESSABLE',
        autoLogout: true
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
