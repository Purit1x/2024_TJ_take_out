import { createStore } from "vuex"
import VuexPersistence from 'vuex-persist';   // 引入持久化插件
// 创建持久化状态实例  
const vuexPersistedState = new VuexPersistence({  
    key: 'my-vuex-store',  
    storage: window.localStorage  
  });  
// 创建持久化状态的实例  
/*const vuexPersistedState = createPersistedState({  
    key: 'my-vuex-store', // 可选，设置要存储在 localStorage 中的键名  
    storage: window.localStorage // 默认使用 localStorage，可以改成 sessionStorage  
  });  */
// 创建一个新的 store 实例@/node_modules/vuex/dist/vuex.esm.js
const store = createStore({
  state: {
      // 用户信息
      user: null,
      merchant:null,
      rider:null,
  },
  mutations: {  
    SET_USER(state, user) {  
        state.user = user;  
    },  
    CLEAR_USER(state) {  
        state.user = null;  
    },  
    SET_MERCHANT(state, merchant) {  
        state.merchant = merchant;  
    },  
    CLEAR_MERCHANT(state) {  
        state.merchant = null;  
    },  
},  
actions: {  
    setUser({ commit }, user) {  
        commit('SET_USER', user);  
    },  
    clearUser({ commit }) {  
        commit('CLEAR_USER');  
    },  
    setMerchant({ commit }, merchant) {  
        commit('SET_MERCHANT', merchant);  
    },  
    clearMerchant({ commit }) {  
        commit('CLEAR_MERCHANT');  
    },  
  },  
  plugins: [vuexPersistedState.plugin] // 使用持久化插件  
})


export default store