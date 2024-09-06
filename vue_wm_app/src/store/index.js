import { createStore } from "vuex"
import VuexPersistence from 'vuex-persist';   // 引入持久化插件
// 创建持久化状态实例  
const vuexPersistedState = new VuexPersistence({  
    key: 'my-vuex-store',  
    storage: window.localStorage,
    rehydrate: (state) => {  
        // 获取时间戳  
        const storedTimestamp = localStorage.getItem('store-timestamp');  
        // 如果存储的时间戳存在，并计算其与当前时间的差值  
        if (storedTimestamp) {  
            const currentTime = new Date().getTime();  
            const timeDiff = currentTime - storedTimestamp;  
  
            // 如果超过一天 (86400000毫秒)  
            if (timeDiff > 86400000) {  
              // 清空存储  
              localStorage.removeItem('my-vuex-store');  
              localStorage.removeItem('store-timestamp');  
              return {}; // 返回空状态  
        }  
      }
      return state; // 返回持久化的状态  
    }  
  });  
const store = createStore({
  state: {
      // 用户信息
      user: null,
      merchant:null,
      rider:null,
      admin:null,
      checkedItems: [], // 新增 checkedItems 状态，代表购物车中的勾选项
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
    SET_RIDER(state, rider) {  
        state.rider = rider;  
    },  
    CLEAR_RIDER(state) {  
        state.rider = null;  
    },  
    SET_ADMIN(state, admin) {  
        state.admin = admin;  
    },  
    CLEAR_ADMIN(state) {  
        state.admin = null;  
    },  
    // 新增 mutations 处理 checkedItems
    SET_CHECKED_ITEMS(state, items) {
        state.checkedItems = items;
    },
    CLEAR_CHECKED_ITEMS(state) {
        state.checkedItems = [];
    },
},  
actions: {  
    setUser({ commit }, user) {  
        commit('SET_USER', user); 
        localStorage.setItem('store-timestamp', new Date().getTime()); // 更新时间戳  
    },  
    clearUser({ commit }) {  
        commit('CLEAR_USER');  
    },  
    setMerchant({ commit }, merchant) {  
        commit('SET_MERCHANT', merchant);  
        localStorage.setItem('store-timestamp', new Date().getTime()); // 更新时间戳 
    },  
    clearMerchant({ commit }) {  
        commit('CLEAR_MERCHANT');  
    },  
    setRider({ commit }, rider) {  
        commit('SET_RIDER', rider);  
        localStorage.setItem('store-timestamp', new Date().getTime()); // 更新时间戳 
    },  
    clearRider({ commit }) {  
        commit('CLEAR_RIDER');  
    },  
    setAdmin({ commit }, admin) {  
        commit('SET_ADMIN', admin);  
        localStorage.setItem('store-timestamp', new Date().getTime()); // 更新时间戳 
    },  
    clearAdmin({ commit }) {  
        commit('CLEAR_ADMIN');  
    },  
    // 新增 actions 处理 checkedItems
    setCheckedItems({ commit }, items) {
        commit('SET_CHECKED_ITEMS', items);
    },
    clearCheckedItems({ commit }) {
        commit('CLEAR_CHECKED_ITEMS');
    }, 
  },  
  plugins: [vuexPersistedState.plugin] // 使用持久化插件  
})


export default store